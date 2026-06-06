#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    public readonly struct MaterialGUIPropertyValidateContext
    {
        public MaterialGUIContext Context { get; }
        public MaterialEditor MaterialEditor => EditorContext.MaterialEditor;
        public Material Material => EditorContext.Material;
        public IReadOnlyList<Material> Targets => EditorContext.Targets;
        public MaterialProperty Property { get; }
        public string Label { get; }
        public PropertyRowLayout Layout { get; }

        public Rect RowRect => Layout.MainRect;
        public Rect LabelRect => Layout.LabelRect;
        public Rect FieldRect => Layout.FieldRect;
        public Rect ResetRect => Layout.ResetRect;
        public Event CurrentEvent => Event.current;

        internal MaterialGUIPropertyValidateContext(
            EditorContext editorContext,
            MaterialProperty property,
            string label,
            in PropertyRowLayout layout)
        {
            Context = editorContext;
            Property = property;
            Label = label ?? string.Empty;
            Layout = layout;
        }

        private EditorContext EditorContext => (EditorContext)Context;

        public void RegisterUndo(string undoName = "Validate Property")
        {
            MaterialEditor?.RegisterPropertyChangeUndo(undoName);
        }

        public void MarkChanged()
        {
            GUI.changed = true;
        }
    }

    internal static class MaterialGUIPropertyValidationRegistry
    {
        private readonly struct ResolvedValidator
        {
            public readonly string Descriptor;
            public readonly Func<MaterialGUIPropertyValidateContext, bool> Callback;

            public ResolvedValidator(string descriptor, Func<MaterialGUIPropertyValidateContext, bool> callback)
            {
                Descriptor = descriptor;
                Callback = callback;
            }
        }

        private static readonly Dictionary<int, Dictionary<string, ResolvedValidator[]>> s_ByShader =
            new();

        private static readonly HashSet<string> s_Warned = new(StringComparer.Ordinal);
        private static bool s_ProjectChangedHooked;

        internal static bool Apply(
            EditorContext ctx,
            MaterialProperty property,
            string label,
            in PropertyRowLayout layout)
        {
            if (ctx == null || property == null) return true;
            if (ctx.Material == null || ctx.Material.shader == null) return true;

            var validators = GetValidators(ctx.Material.shader, property.name);
            if (validators.Length == 0) return true;

            var validateContext = new MaterialGUIPropertyValidateContext(ctx, property, label, layout);
            for (var i = 0; i < validators.Length; i++)
            {
                var v = validators[i];
                try
                {
                    var accepted = v.Callback(validateContext);
                    if (!accepted) return false;
                }
                catch (Exception ex)
                {
                    WarnOnce(
                        $"invoke:{v.Descriptor}:{ex.GetType().FullName}",
                        $"[KIBAMaterialGUI] Validate callback '{v.Descriptor}' threw an exception.\n{ex}");
                }
            }

            return true;
        }

        private static ResolvedValidator[] GetValidators(Shader shader, string propertyName)
        {
            EnsureProjectHook();

            var shaderId = shader.GetInstanceID();
            if (!s_ByShader.TryGetValue(shaderId, out var byProperty))
            {
                byProperty = new Dictionary<string, ResolvedValidator[]>(StringComparer.Ordinal);
                s_ByShader[shaderId] = byProperty;
            }

            if (byProperty.TryGetValue(propertyName, out var cached))
                return cached;

            var built = BuildValidators(shader, propertyName);
            byProperty[propertyName] = built;
            return built;
        }

        private static ResolvedValidator[] BuildValidators(Shader shader, string propertyName)
        {
            if (!ShaderPropertyAttributeCache.TryGetShaderAttributes(shader, propertyName, out var attrs) || attrs.Count == 0)
                return Array.Empty<ResolvedValidator>();

            List<ResolvedValidator>? list = null;
            for (var i = 0; i < attrs.Count; i++)
            {
                var attr = attrs[i];
                if (!string.Equals(attr.name, "Validate", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!TryResolveValidator(attr.args, out var callback, out var descriptor, out var error))
                {
                    WarnOnce(
                        $"resolve:{shader.name}:{propertyName}:{attr.args}",
                        $"[KIBAMaterialGUI] Failed to resolve [Validate] for '{shader.name}/{propertyName}'. {error}");
                    continue;
                }

                list ??= new List<ResolvedValidator>(2);
                list.Add(new ResolvedValidator(descriptor, callback));
            }

            return list is { Count: > 0 } ? list.ToArray() : Array.Empty<ResolvedValidator>();
        }

        internal static bool TryResolveValidator(
            string rawArgs,
            out Func<MaterialGUIPropertyValidateContext, bool> callback,
            out string descriptor,
            out string error)
        {
            callback = null!;
            descriptor = string.Empty;
            error = string.Empty;

            if (!TryParseDescriptor(rawArgs, out var typeName, out var methodName, out var assemblyName))
            {
                error = "Expected format: [Validate(Namespace.Type.Method)] or [Validate(Namespace.Type.Method, AssemblyName)].";
                return false;
            }

            var type = ResolveType(typeName, assemblyName);
            if (type == null)
            {
                error = $"Type '{typeName}' was not found.";
                return false;
            }

            var method = type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(MaterialGUIPropertyValidateContext) },
                null);

            if (method == null || (method.ReturnType != typeof(bool) && method.ReturnType != typeof(void)))
            {
                error = $"Method '{typeName}.{methodName}' must be static and match signature: bool Method(MaterialGUIPropertyValidateContext ctx) or void Method(MaterialGUIPropertyValidateContext ctx).";
                return false;
            }

            if (method.ReturnType == typeof(bool))
            {
                var del = Delegate.CreateDelegate(typeof(Func<MaterialGUIPropertyValidateContext, bool>), method, false)
                          as Func<MaterialGUIPropertyValidateContext, bool>;
                if (del == null)
                {
                    error = $"Failed to bind method '{typeName}.{methodName}' as validator callback.";
                    return false;
                }

                callback = del;
            }
            else
            {
                var del = Delegate.CreateDelegate(typeof(Action<MaterialGUIPropertyValidateContext>), method, false)
                          as Action<MaterialGUIPropertyValidateContext>;
                if (del == null)
                {
                    error = $"Failed to bind method '{typeName}.{methodName}' as validator callback.";
                    return false;
                }

                callback = ctx =>
                {
                    del(ctx);
                    return true;
                };
            }

            if (callback == null)
            {
                error = $"Failed to bind method '{typeName}.{methodName}' as validator callback.";
                return false;
            }

            descriptor = string.IsNullOrWhiteSpace(assemblyName)
                ? $"{typeName}.{methodName}"
                : $"{typeName}.{methodName}, {assemblyName}";
            return true;
        }

        private static bool TryParseDescriptor(
            string rawArgs,
            out string typeName,
            out string methodName,
            out string assemblyName)
        {
            typeName = string.Empty;
            methodName = string.Empty;
            assemblyName = string.Empty;

            var text = TrimQuotes((rawArgs ?? string.Empty).Trim());
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var comma = text.IndexOf(',');
            if (comma >= 0)
            {
                assemblyName = TrimQuotes(text.Substring(comma + 1).Trim());
                text = TrimQuotes(text.Substring(0, comma).Trim());
            }

            var dot = text.LastIndexOf('.');
            if (dot <= 0 || dot >= text.Length - 1)
                return false;

            typeName = text.Substring(0, dot).Trim();
            methodName = text.Substring(dot + 1).Trim();
            return !string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName);
        }

        private static Type? ResolveType(string typeName, string assemblyName)
        {
            if (!string.IsNullOrWhiteSpace(assemblyName))
            {
                var qualified = $"{typeName}, {assemblyName}";
                var typed = Type.GetType(qualified, false, true);
                if (typed != null) return typed;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; i++)
                {
                    var asm = assemblies[i];
                    var asmName = asm.GetName().Name;
                    if (!string.Equals(asmName, assemblyName, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(asm.FullName, assemblyName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    typed = asm.GetType(typeName, false, true);
                    if (typed != null) return typed;
                }
            }

            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; i++)
                {
                    var typed = assemblies[i].GetType(typeName, false, true);
                    if (typed != null) return typed;
                }
            }

            return null;
        }

        private static string TrimQuotes(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Length < 2) return text;
            if ((text[0] == '"' && text[^1] == '"') || (text[0] == '\'' && text[^1] == '\''))
                return text.Substring(1, text.Length - 2).Trim();
            return text;
        }

        private static void EnsureProjectHook()
        {
            if (s_ProjectChangedHooked) return;
            s_ProjectChangedHooked = true;
            EditorApplication.projectChanged += Invalidate;
        }

        private static void Invalidate()
        {
            s_ByShader.Clear();
            s_Warned.Clear();
        }

        private static void WarnOnce(string key, string message)
        {
            if (!s_Warned.Add(key)) return;
            Debug.LogWarning(message);
        }
    }
}


