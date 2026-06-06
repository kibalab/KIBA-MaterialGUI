#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    internal static class MaterialGUIPropertyRendererRegistry
    {
        private static bool s_Scanned;
        private static readonly List<Entry> Entries = new();

#if UNITY_INCLUDE_TESTS
        internal static void ResetForTests()
        {
            s_Scanned = false;
            Entries.Clear();
            MaterialGUIRegistryDiagnostics.ResetForTests();
        }
#endif

        private sealed class Entry
        {
            public int Order;
            public IMaterialGUIPropertyRenderer? Impl;
            public string[] ShaderNameEquals = Array.Empty<string>();
            public string[] ShaderNameContains = Array.Empty<string>();
            public Regex? ShaderNameRegex;
            public string[] RequireProperties = Array.Empty<string>();
            public string[] RequireKeywords = Array.Empty<string>();
            public string? PropertyName;
            public MaterialProperty.PropType[] PropertyTypes = Array.Empty<MaterialProperty.PropType>();
            public string[] RequireShaderAttributes = Array.Empty<string>();
            public string[] ExcludeShaderAttributes = Array.Empty<string>();
        }

        private static void EnsureScan()
        {
            if (s_Scanned) return;
            s_Scanned = true;

            var types = TypeCache.GetTypesWithAttribute<MaterialGUIPropertyRendererAttribute>();
            foreach (var type in types)
            {
                if (type == null || type.IsAbstract) continue;
                if (!ValidateRendererType(type)) continue;

                var attrs = (MaterialGUIPropertyRendererAttribute[])type.GetCustomAttributes(typeof(MaterialGUIPropertyRendererAttribute), false);
                foreach (var attr in attrs)
                {
                    if (attr == null) continue;
                    var inst = CreateInstance(type);

                    if (inst == null) continue;
                    if (!TryCreateEntry(attr, inst, out var entry) || entry == null)
                        continue;
                    Entries.Add(entry);
                }
            }

            Entries.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        }

        private static bool ValidateRendererType(Type type)
        {
            if (typeof(IMaterialGUIPropertyRenderer).IsAssignableFrom(type))
                return true;

            MaterialGUIRegistryDiagnostics.WarnOnce(
                $"renderer.interface:{type.FullName}",
                $"Property renderer declaration '{type.FullName}' has [MaterialGUIPropertyRenderer] but does not implement IMaterialGUIPropertyRenderer.");
            return false;
        }

        private static IMaterialGUIPropertyRenderer? CreateInstance(Type type)
        {
            try
            {
                return (IMaterialGUIPropertyRenderer?)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                MaterialGUIRegistryDiagnostics.WarnOnce(
                    $"renderer.create:{type.FullName}:{ex.GetType().FullName}",
                    $"Failed to create property renderer '{type.FullName}'. {ex.Message}");
                return null;
            }
        }

        public static bool TryFindRenderer(PropertyRendererArgs args, out IMaterialGUIPropertyRenderer? renderer)
        {
            EnsureScan();
            renderer = null;

            for (var idx = 0; idx < Entries.Count; idx++)
            {
                var entry = Entries[idx];
                var impl = entry.Impl!;

                if (!MatchShader(entry, args)) continue;
                if (!MatchProps(entry, args)) continue;
                if (!MatchKeywords(entry, args)) continue;
                if (!MatchPropertyName(entry, args)) continue;
                if (!MatchPropertyType(entry, args)) continue;
                if (!MatchShaderAttributes(entry, args)) continue;
                if (impl is IMaterialGUIPropertyRendererFilter filter && !filter.CanRender(args)) continue;

                renderer = impl;
                return true;
            }

            return false;
        }


        private static bool MatchShader(Entry entry, PropertyRendererArgs args)
        {
            var shaderName = args.Shader != null ? args.Shader.name : string.Empty;

            if (entry.ShaderNameEquals.Length > 0)
            {
                var matched = false;
                for (var i = 0; i < entry.ShaderNameEquals.Length; i++)
                {
                    if (!string.Equals(entry.ShaderNameEquals[i], shaderName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    matched = true;
                    break;
                }

                if (!matched)
                    return false;
            }

            if (entry.ShaderNameContains.Length > 0)
            {
                var matched = false;
                for (var i = 0; i < entry.ShaderNameContains.Length; i++)
                {
                    if (shaderName.IndexOf(entry.ShaderNameContains[i], StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    matched = true;
                    break;
                }

                if (!matched)
                    return false;
            }

            if (entry.ShaderNameRegex != null)
            {
                if (!entry.ShaderNameRegex.IsMatch(shaderName))
                    return false;
            }

            return true;
        }

        private static bool MatchProps(Entry entry, PropertyRendererArgs args)
        {
            if (entry.RequireProperties.Length == 0) return true;
            if (args.Material == null) return false;

            for (var i = 0; i < entry.RequireProperties.Length; i++)
            {
                if (!args.Material.HasProperty(entry.RequireProperties[i]))
                    return false;
            }

            return true;
        }

        private static bool MatchKeywords(Entry entry, PropertyRendererArgs args)
        {
            if (entry.RequireKeywords.Length == 0) return true;
            if (args.Material == null) return false;

            for (var i = 0; i < entry.RequireKeywords.Length; i++)
            {
                if (!args.Material.IsKeywordEnabled(entry.RequireKeywords[i]))
                    return false;
            }

            return true;
        }

        private static bool MatchPropertyName(Entry entry, PropertyRendererArgs args)
        {
            if (string.IsNullOrWhiteSpace(entry.PropertyName)) return true;
            return string.Equals(entry.PropertyName, args.Property.name, StringComparison.Ordinal);
        }

        private static bool MatchPropertyType(Entry entry, PropertyRendererArgs args)
        {
            if (entry.PropertyTypes.Length == 0) return true;

            for (var i = 0; i < entry.PropertyTypes.Length; i++)
            {
                if (entry.PropertyTypes[i] == args.Property.type)
                    return true;
            }

            return false;
        }

        private static bool MatchShaderAttributes(Entry entry, PropertyRendererArgs args)
        {
            if (entry.RequireShaderAttributes.Length > 0)
            {
                for (var i = 0; i < entry.RequireShaderAttributes.Length; i++)
                {
                    if (!args.HasShaderAttribute(entry.RequireShaderAttributes[i]))
                        return false;
                }
            }

            if (entry.ExcludeShaderAttributes.Length > 0)
            {
                for (var i = 0; i < entry.ExcludeShaderAttributes.Length; i++)
                {
                    if (args.HasShaderAttribute(entry.ExcludeShaderAttributes[i]))
                        return false;
                }
            }

            return true;
        }

        private static bool TryCreateEntry(
            MaterialGUIPropertyRendererAttribute attr,
            IMaterialGUIPropertyRenderer impl,
            out Entry? entry)
        {
            entry = null;

            Regex? regex = null;
            if (!string.IsNullOrWhiteSpace(attr.ShaderNameRegex))
            {
                try
                {
                    regex = new Regex(
                        attr.ShaderNameRegex,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                catch (Exception ex)
                {
                    MaterialGUIRegistryDiagnostics.WarnOnce(
                        $"renderer.regex:{impl.GetType().FullName}:{attr.ShaderNameRegex}",
                        $"Invalid ShaderNameRegex on property renderer '{impl.GetType().FullName}': {ex.Message}");
                    return false;
                }
            }

            entry = new Entry
            {
                Order = attr.Order,
                Impl = impl,
                ShaderNameEquals = Normalize(attr.ShaderNameEquals),
                ShaderNameContains = Normalize(attr.ShaderNameContains),
                ShaderNameRegex = regex,
                RequireProperties = Normalize(attr.RequireProperties),
                RequireKeywords = Normalize(attr.RequireKeywords),
                PropertyName = string.IsNullOrWhiteSpace(attr.PropertyName) ? null : attr.PropertyName,
                PropertyTypes = attr.PropertyTypes ?? Array.Empty<MaterialProperty.PropType>(),
                RequireShaderAttributes = Normalize(attr.RequireShaderAttributes),
                ExcludeShaderAttributes = Normalize(attr.ExcludeShaderAttributes),
            };
            return true;
        }

#if UNITY_INCLUDE_TESTS
        internal static bool ValidateRendererTypeForTests(Type type)
        {
            return ValidateRendererType(type);
        }

        internal static bool TryCreateInstanceForTests(Type type)
        {
            return CreateInstance(type) != null;
        }

        internal static bool TryCreateEntryForTests(
            MaterialGUIPropertyRendererAttribute attr,
            IMaterialGUIPropertyRenderer impl)
        {
            return TryCreateEntry(attr, impl, out _);
        }
#endif

        private static string[] Normalize(string[]? values)
        {
            if (values == null || values.Length == 0) return Array.Empty<string>();

            var count = 0;
            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    count++;
            }

            if (count == 0) return Array.Empty<string>();

            var normalized = new string[count];
            var index = 0;
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (string.IsNullOrWhiteSpace(value)) continue;
                normalized[index++] = value;
            }

            return normalized;
        }
    }
}


