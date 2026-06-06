#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    internal static class ExtensionRegistry
    {
        private static bool _scanned;
        private static readonly List<Entry>[] EntriesByHook = CreateBuckets();

        private sealed class Entry
        {
            public readonly int Order;
            public readonly IShaderEditor Impl;
            public readonly string[] ShaderNameEquals;
            public readonly string[] ShaderNameContains;
            public readonly Regex? ShaderRegex;
            public readonly string[] RequireProperties;
            public readonly string[] RequireKeywords;
            public readonly string? GroupPath;
            public readonly string? PropertyName;

            public Entry(
                int order,
                IShaderEditor impl,
                string[] shaderNameEquals,
                string[] shaderNameContains,
                Regex? shaderRegex,
                string[] requireProperties,
                string[] requireKeywords,
                string? groupPath,
                string? propertyName)
            {
                Order = order;
                Impl = impl;
                ShaderNameEquals = shaderNameEquals;
                ShaderNameContains = shaderNameContains;
                ShaderRegex = shaderRegex;
                RequireProperties = requireProperties;
                RequireKeywords = requireKeywords;
                GroupPath = groupPath;
                PropertyName = propertyName;
            }
        }

        private static List<Entry>[] CreateBuckets()
        {
            var len = Enum.GetValues(typeof(HookPoint)).Length;
            var buckets = new List<Entry>[len];
            for (var i = 0; i < len; i++)
                buckets[i] = new List<Entry>();
            return buckets;
        }

        private static void EnsureScan()
        {
            if (_scanned) return;
            _scanned = true;

            var types = TypeCache.GetTypesWithAttribute<ShaderEditorInjectionAttribute>();
            for (var i = 0; i < types.Count; i++)
            {
                var t = types[i];
                if (t == null || t.IsAbstract) continue;
                if (!ValidateExtensionType(t)) continue;

                var attrs = (ShaderEditorInjectionAttribute[])t.GetCustomAttributes(typeof(ShaderEditorInjectionAttribute), false);
                if (attrs == null || attrs.Length == 0) continue;

                var impl = CreateInstance(t);
                if (impl == null) continue;

                for (var j = 0; j < attrs.Length; j++)
                {
                    var attr = attrs[j];
                    if (attr == null) continue;
                    if (!TryCreateEntry(attr, impl, out var entry) || entry == null) continue;

                    var hookIndex = (int)attr.Hook;
                    if ((uint)hookIndex >= (uint)EntriesByHook.Length) continue;

                    EntriesByHook[hookIndex].Add(entry);
                }
            }

            for (var i = 0; i < EntriesByHook.Length; i++)
                EntriesByHook[i].Sort(static (a, b) => a.Order.CompareTo(b.Order));
        }

        public static void Draw(HookPoint hook, InjectionArgs args)
        {
            EnsureScan();

            var ctx = args.InternalContext;
            if (ctx?.Material == null) return;

            var hookIndex = (int)hook;
            if ((uint)hookIndex >= (uint)EntriesByHook.Length) return;

            var shaderName = ctx.Material.shader != null ? ctx.Material.shader.name : string.Empty;
            var entries = EntriesByHook[hookIndex];
            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (!MatchShader(e, shaderName)) continue;
                if (!MatchProperties(e, ctx)) continue;
                if (!MatchKeywords(e, ctx)) continue;
                if (!MatchGroup(e.GroupPath, args.GroupPath)) continue;
                if (!MatchProperty(e.PropertyName, args.Property)) continue;

                e.Impl.OnGUI(args);
            }
        }

        private static bool ValidateExtensionType(Type type)
        {
            if (typeof(IShaderEditor).IsAssignableFrom(type))
                return true;

            MaterialGUIRegistryDiagnostics.WarnOnce(
                $"extension.interface:{type.FullName}",
                $"Shader editor injection declaration '{type.FullName}' has [ShaderEditorInjection] but does not implement IShaderEditor.");
            return false;
        }

        private static IShaderEditor? CreateInstance(Type type)
        {
            try
            {
                return (IShaderEditor)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                MaterialGUIRegistryDiagnostics.WarnOnce(
                    $"extension.create:{type.FullName}:{ex.GetType().FullName}",
                    $"Failed to create shader editor extension '{type.FullName}'. {ex.Message}");
                return null;
            }
        }

        private static bool TryCreateEntry(ShaderEditorInjectionAttribute attr, IShaderEditor impl, out Entry? entry)
        {
            var shaderEquals = Normalize(attr.ShaderNameEquals);
            var shaderContains = Normalize(attr.ShaderNameContains);
            var requireProperties = Normalize(attr.RequireProperties);
            var requireKeywords = Normalize(attr.RequireKeywords);

            Regex? regex = null;
            if (!string.IsNullOrEmpty(attr.ShaderNameRegex))
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
                        $"extension.regex:{impl.GetType().FullName}:{attr.ShaderNameRegex}",
                        $"Invalid ShaderNameRegex on shader editor extension '{impl.GetType().FullName}': {ex.Message}");
                    entry = null;
                    return false;
                }
            }

            entry = new Entry(
                attr.Order,
                impl,
                shaderEquals,
                shaderContains,
                regex,
                requireProperties,
                requireKeywords,
                string.IsNullOrWhiteSpace(attr.GroupPath) ? null : attr.GroupPath,
                string.IsNullOrWhiteSpace(attr.PropertyName) ? null : attr.PropertyName);
            return true;
        }

#if UNITY_INCLUDE_TESTS
        internal static bool ValidateExtensionTypeForTests(Type type)
        {
            return ValidateExtensionType(type);
        }

        internal static bool TryCreateInstanceForTests(Type type)
        {
            return CreateInstance(type) != null;
        }

        internal static bool TryCreateEntryForTests(ShaderEditorInjectionAttribute attr, IShaderEditor impl)
        {
            return TryCreateEntry(attr, impl, out _);
        }
#endif

        private static bool MatchShader(Entry entry, string shaderName)
        {
            if (!MatchEqualsFilter(entry.ShaderNameEquals, shaderName))
                return false;
            if (!MatchContainsFilter(entry.ShaderNameContains, shaderName))
                return false;
            if (entry.ShaderRegex != null && !entry.ShaderRegex.IsMatch(shaderName))
                return false;
            return true;
        }

        private static bool MatchProperties(Entry entry, EditorContext ctx)
        {
            var required = entry.RequireProperties;
            if (required.Length == 0) return true;

            var targetCount = GetTargetCount(ctx);
            if (targetCount == 0) return false;

            for (var t = 0; t < targetCount; t++)
            {
                var mat = GetTargetAt(ctx, t);
                if (mat == null) return false;

                for (var i = 0; i < required.Length; i++)
                {
                    if (!mat.HasProperty(required[i]))
                        return false;
                }
            }

            return true;
        }

        private static bool MatchKeywords(Entry entry, EditorContext ctx)
        {
            var required = entry.RequireKeywords;
            if (required.Length == 0) return true;

            var targetCount = GetTargetCount(ctx);
            if (targetCount == 0) return false;

            for (var t = 0; t < targetCount; t++)
            {
                var mat = GetTargetAt(ctx, t);
                if (mat == null) return false;

                for (var i = 0; i < required.Length; i++)
                {
                    if (!mat.IsKeywordEnabled(required[i]))
                        return false;
                }
            }

            return true;
        }

        private static bool MatchGroup(string? requiredGroupPath, string? currentGroupPath)
        {
            if (string.IsNullOrEmpty(requiredGroupPath)) return true;
            if (string.IsNullOrEmpty(currentGroupPath)) return false;
            return string.Equals(requiredGroupPath, currentGroupPath, StringComparison.Ordinal);
        }

        private static bool MatchProperty(string? requiredPropertyName, MaterialProperty? current)
        {
            if (string.IsNullOrEmpty(requiredPropertyName)) return true;
            return current != null && string.Equals(requiredPropertyName, current.name, StringComparison.Ordinal);
        }

        private static bool MatchEqualsFilter(string[] filters, string value)
        {
            if (filters.Length == 0) return true;

            for (var i = 0; i < filters.Length; i++)
            {
                if (string.Equals(filters[i], value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool MatchContainsFilter(string[] filters, string value)
        {
            if (filters.Length == 0) return true;

            for (var i = 0; i < filters.Length; i++)
            {
                if (value.IndexOf(filters[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

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
                var v = values[i];
                if (string.IsNullOrWhiteSpace(v)) continue;
                normalized[index++] = v;
            }

            return normalized;
        }

        private static int GetTargetCount(EditorContext ctx)
        {
            var targets = ctx.Targets;
            if (targets != null && targets.Count > 0) return targets.Count;
            return ctx.Material != null ? 1 : 0;
        }

        private static Material? GetTargetAt(EditorContext ctx, int index)
        {
            var targets = ctx.Targets;
            if (targets != null && targets.Count > 0)
            {
                if ((uint)index >= (uint)targets.Count) return null;
                return targets[index];
            }

            return index == 0 ? ctx.Material : null;
        }
    }
}


