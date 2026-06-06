#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Localization;
using KIBA_.KIBAMaterialGUI.Editor.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    internal static class MaterialGUIModelBuilder
    {
        private static readonly HashSet<string> KnownMaterialGUIAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Group",
            "GradientTexture",
            "Gradient",
            "FlexibleRange",
            "Flexible",
            "Space",
            "Divider",
            "SegmentedEnum",
            "Segmented",
            "Unit",
            "MinMaxSlider",
            "Vector",
            "VectorN",
            "VectorFields",
            "Vector2",
            "Vector3",
            "Vector4",
            "Validate",
            "ShowIf",
        };

        private static readonly HashSet<string> KnownUnityAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Enum",
            "KeywordEnum",
            "Toggle",
            "ToggleOff",
            "ToggleUI",
            "MaterialToggle",
            "NoScaleOffset",
            "Normal",
            "HDR",
            "Gamma",
            "PowerSlider",
            "IntRange",
            "PerRendererData",
            "MainTexture",
            "MainColor",
            "HideInInspector",
        };

        internal static MaterialGUIModel Build(EditorContext ctx)
        {
            var root = new GroupNodeModel { Name = "ROOT", PathKey = "ROOT", Expanded = true };
            var allProperties = new List<ShaderPropertyModel>(ctx.Properties.Count);
            var diagnostics = new List<MaterialGUIDiagnostic>();
            var shader = ctx.Material != null ? ctx.Material.shader : null;

            Material? defaultMaterial = null;
            try
            {
                if (shader != null)
                    defaultMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

                var properties = ctx.Properties;
                for (var i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    if (property == null) continue;

                    ctx.DisplayParser.ParseDisplay(property.displayName, out _, out var rawLabel);
                    var label = string.IsNullOrEmpty(rawLabel) ? property.name : rawLabel;
                    var groupPath = ResolveGroupPath(shader, property.name);
                    var attrs = GetAttributes(shader, property.name);

                    var model = new ShaderPropertyModel
                    {
                        Property = property,
                        PropertyName = property.name,
                        Label = label,
                        TranslatedLabel = LocalizationHelper.TranslateProp(ctx, property.name, label),
                        GroupPath = groupPath,
                        PropertyType = property.type,
                        Attributes = attrs,
                        Changed = IsChanged(ctx, defaultMaterial, property),
                        Mixed = property.hasMixedValue,
                    };

                    ApplyConditionalVisibility(ctx, model);
                    AddDiagnostics(ctx, shader, model);
                    allProperties.Add(model);
                    AddToTree(ctx, root, model);
                }
            }
            finally
            {
                if (defaultMaterial != null)
                    Object.DestroyImmediate(defaultMaterial);
            }

            ApplyFilter(ctx, root);
            CollectDiagnostics(root, diagnostics);
            return new MaterialGUIModel(root, allProperties, diagnostics);
        }

        internal static void ApplyFilter(EditorContext ctx, GroupNodeModel root)
        {
            var search = ctx.State?.Search ?? ctx.Search ?? string.Empty;
            var filters = ctx.State?.Filters ?? MaterialGUIFilter.None;
            ApplyFilterRecursive(ctx, root, search, filters, true);
        }

        private static bool ApplyFilterRecursive(
            EditorContext ctx,
            GroupNodeModel node,
            string search,
            MaterialGUIFilter filters,
            bool isRoot)
        {
            node.TotalPropertyCount = 0;
            node.VisiblePropertyCount = 0;
            node.ChangedCount = 0;
            node.WarningCount = 0;
            node.HasMixed = false;
            node.SearchMatchedSelf = MatchesNodeSearch(ctx, node, search);

            var anyVisible = false;
            for (var i = 0; i < node.Properties.Count; i++)
            {
                var property = node.Properties[i];
                property.SearchMatched = MatchesSearch(ctx, property, search);
                property.Visible = property.ConditionVisible && property.SearchMatched && PassFilters(property, filters);
                if (property.ConditionVisible)
                    node.TotalPropertyCount++;

                if (property.Changed) node.ChangedCount++;
                if (property.WarningCount > 0) node.WarningCount += property.WarningCount;
                if (property.Mixed) node.HasMixed = true;

                if (!property.Visible) continue;
                node.VisiblePropertyCount++;
                anyVisible = true;
            }

            foreach (var child in node.Children.Values)
            {
                var childVisible = ApplyFilterRecursive(ctx, child, search, filters, false);
                node.TotalPropertyCount += child.TotalPropertyCount;
                node.VisiblePropertyCount += child.VisiblePropertyCount;
                node.ChangedCount += child.ChangedCount;
                node.WarningCount += child.WarningCount;
                node.HasMixed |= child.HasMixed;
                anyVisible |= childVisible;
            }

            var hasQuery = !string.IsNullOrWhiteSpace(search);
            node.Visible = isRoot || anyVisible || (hasQuery && node.SearchMatchedSelf);
            return node.Visible;
        }

        private static void AddToTree(EditorContext ctx, GroupNodeModel root, ShaderPropertyModel property)
        {
            if (string.IsNullOrEmpty(property.GroupPath))
            {
                root.MutableProperties.Add(property);
                return;
            }

            var parts = property.GroupPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                root.MutableProperties.Add(property);
                return;
            }

            var node = root;
            var acc = string.Empty;
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                acc = string.IsNullOrEmpty(acc) ? part : $"{acc}/{part}";
                if (!node.MutableChildren.TryGetValue(part, out var child))
                {
                    child = new GroupNodeModel
                    {
                        Name = part,
                        PathKey = acc,
                        Expanded = FoldState.GetFold(ctx.PreferencesKeyPrefix, acc, true)
                    };
                    node.MutableChildren.Add(part, child);
                }

                node = child;
            }

            node.MutableProperties.Add(property);
        }

        private static string ResolveGroupPath(Shader? shader, string propertyName)
        {
            if (ShaderPropertyAttributeCache.TryGetGroupPath(shader, propertyName, out var attrPath) &&
                !string.IsNullOrWhiteSpace(attrPath))
                return attrPath;
            return string.Empty;
        }

        private static IReadOnlyList<ShaderPropertyAttributeCache.ShaderAttributeInfo> GetAttributes(Shader? shader, string propertyName)
        {
            return ShaderPropertyAttributeCache.TryGetShaderAttributes(shader, propertyName, out var attrs)
                ? attrs
                : Array.Empty<ShaderPropertyAttributeCache.ShaderAttributeInfo>();
        }

        private static bool MatchesNodeSearch(EditorContext ctx, GroupNodeModel node, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return true;
            return ContainsToken(node.Name, search) ||
                   ContainsToken(node.PathKey, search) ||
                   ContainsToken(LocalizationHelper.TranslateGroup(ctx, node.Name, node.PathKey), search);
        }

        private static bool MatchesSearch(EditorContext ctx, ShaderPropertyModel model, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return true;

            if (ContainsToken(model.PropertyName, search) ||
                ContainsToken(model.Label, search) ||
                ContainsToken(model.TranslatedLabel, search) ||
                ContainsToken(model.GroupPath, search))
                return true;

            var attrs = model.Attributes;
            for (var i = 0; i < attrs.Count; i++)
            {
                var attr = attrs[i];
                if (ContainsToken(attr.name, search) ||
                    ContainsToken(attr.args, search) ||
                    ContainsToken(attr.raw, search))
                    return true;
            }

            return false;
        }

        private static bool PassFilters(ShaderPropertyModel model, MaterialGUIFilter filters)
        {
            if ((filters & MaterialGUIFilter.Changed) != 0 && !model.Changed) return false;
            if ((filters & MaterialGUIFilter.Warnings) != 0 && model.WarningCount == 0) return false;

            var typeFilter =
                (filters & (MaterialGUIFilter.Textures | MaterialGUIFilter.Numbers | MaterialGUIFilter.Colors)) != 0;
            if (!typeFilter) return true;

            if ((filters & MaterialGUIFilter.Textures) != 0 &&
                model.PropertyType == MaterialProperty.PropType.Texture)
                return true;
            if ((filters & MaterialGUIFilter.Colors) != 0 &&
                model.PropertyType == MaterialProperty.PropType.Color)
                return true;
            if ((filters & MaterialGUIFilter.Numbers) != 0 &&
                (model.PropertyType == MaterialProperty.PropType.Float ||
                 model.PropertyType == MaterialProperty.PropType.Range ||
                 model.PropertyType == MaterialProperty.PropType.Vector))
                return true;

            return false;
        }

        private static bool ContainsToken(string? value, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return true;
            return (value ?? string.Empty).IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsChanged(EditorContext ctx, Material? defaultMaterial, MaterialProperty property)
        {
            if (defaultMaterial == null || string.IsNullOrWhiteSpace(property.name)) return false;

            var targets = ctx.Targets.Count > 0
                ? ctx.Targets
                : ctx.Material != null
                    ? new[] { ctx.Material }
                    : Array.Empty<Material>();

            for (var i = 0; i < targets.Count; i++)
            {
                var mat = targets[i];
                if (mat == null || !mat.HasProperty(property.name)) continue;
                if (PropertyDiffers(mat, defaultMaterial, property))
                    return true;
            }

            return false;
        }

        private static bool PropertyDiffers(Material mat, Material defaultMaterial, MaterialProperty property)
        {
            if (!defaultMaterial.HasProperty(property.name)) return false;

            switch (property.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    return !Mathf.Approximately(mat.GetFloat(property.name), defaultMaterial.GetFloat(property.name));
                case MaterialProperty.PropType.Color:
                    return mat.GetColor(property.name) != defaultMaterial.GetColor(property.name);
                case MaterialProperty.PropType.Vector:
                    return mat.GetVector(property.name) != defaultMaterial.GetVector(property.name);
                case MaterialProperty.PropType.Texture:
                    if (mat.GetTexture(property.name) != defaultMaterial.GetTexture(property.name)) return true;
                    return mat.GetTextureScale(property.name) != defaultMaterial.GetTextureScale(property.name) ||
                           mat.GetTextureOffset(property.name) != defaultMaterial.GetTextureOffset(property.name);
                default:
                    return false;
            }
        }

        private static void ApplyConditionalVisibility(EditorContext ctx, ShaderPropertyModel model)
        {
            model.ConditionVisible = true;
            model.ConditionalVisibility = default;

            if (!TryGetShowIf(model, out var attr))
                return;

            if (!TryParseShowIf(attr.args, out var sourcePropertyName, out var expectedValue, out var error))
            {
                model.ConditionalVisibility = new ConditionalVisibilityInfo(true, sourcePropertyName, expectedValue, true, false);
                model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                    MaterialGUIDiagnosticSeverity.Warning,
                    string.IsNullOrWhiteSpace(error) ? "[ShowIf] has malformed arguments." : error,
                    model.PropertyName,
                    model.GroupPath));
                return;
            }

            if (!TryEvaluateShowIf(ctx, sourcePropertyName, expectedValue, out var satisfied, out error))
            {
                model.ConditionalVisibility = new ConditionalVisibilityInfo(true, sourcePropertyName, expectedValue, true, false);
                model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                    MaterialGUIDiagnosticSeverity.Warning,
                    error,
                    model.PropertyName,
                    model.GroupPath));
                return;
            }

            model.ConditionVisible = satisfied;
            model.ConditionalVisibility = new ConditionalVisibilityInfo(true, sourcePropertyName, expectedValue, satisfied, true);
        }

        private static bool TryGetShowIf(
            ShaderPropertyModel model,
            out ShaderPropertyAttributeCache.ShaderAttributeInfo attr)
        {
            var attrs = model.Attributes;
            for (var i = 0; i < attrs.Count; i++)
            {
                var candidate = attrs[i];
                if (!candidate.name.Equals("ShowIf", StringComparison.OrdinalIgnoreCase)) continue;
                attr = candidate;
                return true;
            }

            attr = default;
            return false;
        }

        private static bool TryParseShowIf(
            string args,
            out string sourcePropertyName,
            out float expectedValue,
            out string error)
        {
            sourcePropertyName = string.Empty;
            expectedValue = 1f;
            error = string.Empty;

            var tokens = SplitArgs(args);
            if (tokens.Length == 0)
            {
                error = "[ShowIf] expects a controller property name.";
                return false;
            }

            if (tokens.Length > 2)
            {
                error = "[ShowIf] supports only property name and optional expected value.";
                return false;
            }

            sourcePropertyName = tokens[0];
            if (string.IsNullOrWhiteSpace(sourcePropertyName))
            {
                error = "[ShowIf] controller property name is empty.";
                return false;
            }

            if (tokens.Length == 1)
                return true;

            if (float.TryParse(
                    tokens[1],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out expectedValue))
                return true;

            error = "[ShowIf] expected value must be numeric.";
            return false;
        }

        private static bool TryEvaluateShowIf(
            EditorContext ctx,
            string sourcePropertyName,
            float expectedValue,
            out bool satisfied,
            out string error)
        {
            satisfied = true;
            error = string.Empty;

            var controller = FindProperty(ctx.Properties, sourcePropertyName);
            if (controller == null)
            {
                error = $"[ShowIf] controller property '{sourcePropertyName}' was not found.";
                return false;
            }

            if (controller.type != MaterialProperty.PropType.Float &&
                controller.type != MaterialProperty.PropType.Range)
            {
                error = $"[ShowIf] controller property '{sourcePropertyName}' must be Float or Range.";
                return false;
            }

            var targets = ctx.Targets.Count > 0
                ? ctx.Targets
                : ctx.Material != null
                    ? new[] { ctx.Material }
                    : Array.Empty<Material>();

            if (targets.Count == 0)
            {
                satisfied = Mathf.Approximately(controller.floatValue, expectedValue);
                return true;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                var mat = targets[i];
                if (mat == null || !mat.HasProperty(sourcePropertyName)) continue;
                if (Mathf.Approximately(mat.GetFloat(sourcePropertyName), expectedValue))
                {
                    satisfied = true;
                    return true;
                }
            }

            satisfied = false;
            return true;
        }

        private static MaterialProperty? FindProperty(IReadOnlyList<MaterialProperty> properties, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) return null;

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                if (property != null && string.Equals(property.name, propertyName, StringComparison.Ordinal))
                    return property;
            }

            return null;
        }

        private static void AddDiagnostics(EditorContext ctx, Shader? shader, ShaderPropertyModel model)
        {
            var attrs = model.Attributes;
            for (var i = 0; i < attrs.Count; i++)
            {
                var attr = attrs[i];
                AddUnknownAttributeDiagnostic(model, attr);
                AddInvalidKnownAttributeDiagnostic(model, attr);
            }

            if (ctx.LocalizationStore != null &&
                !string.IsNullOrEmpty(ctx.CurrentLanguage) &&
                !string.IsNullOrEmpty(model.PropertyName) &&
                string.Equals(model.TranslatedLabel, model.Label, StringComparison.Ordinal))
            {
                model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                    MaterialGUIDiagnosticSeverity.Info,
                    $"No localization entry for property '{model.PropertyName}'.",
                    model.PropertyName,
                    model.GroupPath));
            }
        }

        private static void AddUnknownAttributeDiagnostic(
            ShaderPropertyModel model,
            ShaderPropertyAttributeCache.ShaderAttributeInfo attr)
        {
            if (string.IsNullOrWhiteSpace(attr.name)) return;
            if (KnownMaterialGUIAttributes.Contains(attr.name) || KnownUnityAttributes.Contains(attr.name)) return;

            if (!attr.name.StartsWith("Path", StringComparison.OrdinalIgnoreCase) &&
                !attr.name.StartsWith("PD", StringComparison.OrdinalIgnoreCase))
                return;

            model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                MaterialGUIDiagnosticSeverity.Warning,
                $"Unknown KIBAMaterialGUI-style attribute '{attr.raw}'.",
                model.PropertyName,
                model.GroupPath));
        }

        private static void AddInvalidKnownAttributeDiagnostic(
            ShaderPropertyModel model,
            ShaderPropertyAttributeCache.ShaderAttributeInfo attr)
        {
            if (attr.name.Equals("Group", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(model.GroupPath))
            {
                model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                    MaterialGUIDiagnosticSeverity.Warning,
                    "[Group] has no usable path.",
                    model.PropertyName,
                    model.GroupPath));
                return;
            }

            if ((attr.name.Equals("Vector", StringComparison.OrdinalIgnoreCase) ||
                 attr.name.Equals("VectorN", StringComparison.OrdinalIgnoreCase) ||
                 attr.name.Equals("VectorFields", StringComparison.OrdinalIgnoreCase)) &&
                !TryParseIntArg(attr.args, out var vectorCount, 2, 4))
            {
                model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                    MaterialGUIDiagnosticSeverity.Warning,
                    $"[{attr.name}] expects a field count from 2 to 4.",
                    model.PropertyName,
                    model.GroupPath));
                return;
            }

            if (attr.name.Equals("MinMaxSlider", StringComparison.OrdinalIgnoreCase) &&
                SplitArgs(attr.args).Length < 2)
            {
                model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                    MaterialGUIDiagnosticSeverity.Warning,
                    "[MinMaxSlider] expects min and max arguments.",
                    model.PropertyName,
                    model.GroupPath));
                return;
            }

            if (attr.name.Equals("Validate", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(attr.args))
            {
                model.MutableDiagnostics.Add(new MaterialGUIDiagnostic(
                    MaterialGUIDiagnosticSeverity.Warning,
                    "[Validate] expects a method path.",
                    model.PropertyName,
                    model.GroupPath));
            }
        }

        private static bool TryParseIntArg(string args, out int value, int min, int max)
        {
            value = 0;
            var tokens = SplitArgs(args);
            if (tokens.Length == 0) return false;
            if (!float.TryParse(tokens[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f))
                return false;
            value = Mathf.RoundToInt(f);
            return value >= min && value <= max;
        }

        private static string[] SplitArgs(string args)
        {
            if (string.IsNullOrWhiteSpace(args)) return Array.Empty<string>();
            return args.Split(',')
                .Select(static s => s.Trim().Trim('"', '\''))
                .Where(static s => s.Length > 0)
                .ToArray();
        }

        private static void CollectDiagnostics(GroupNodeModel root, List<MaterialGUIDiagnostic> diagnostics)
        {
            foreach (var property in root.Properties)
            {
                for (var i = 0; i < property.Diagnostics.Count; i++)
                    diagnostics.Add(property.Diagnostics[i]);
            }

            foreach (var child in root.Children.Values)
                CollectDiagnostics(child, diagnostics);
        }
    }
}


