#nullable enable

using System.Collections.Generic;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    internal static class PropertyRowHost
    {
        private const float DividerTopPadding = 2f;
        private const float DividerBottomPadding = 4f;
        private const float DividerLineHeight = 1f;
        private const float DepthIndent = 14f;
        private const float ResetWidth = 18f;
        private const float ResetSpacing = 4f;
        private static readonly HashSet<MaterialProperty> RegisteredPropertyChanges = new();

        public static void Draw(EditorContext ctx, MaterialProperty property, string label, int depth)
        {
            if (ctx == null || property == null) return;

            RegisteredPropertyChanges.Clear();

            float height;
            MaterialPropertyRendererRegistry.ResolvedPropertyRenderer resolved;
            var propertyModel = FindModel(ctx, property);
            using (new EditorGUIWideModeScope(true))
            {
                var probeArgs = new PropertyRendererArgs(
                    Rect.zero,
                    ctx.MaterialEditor,
                    ctx.Material,
                    property,
                    label,
                    ctx.Styles.MiniGray,
                    default,
                    propertyModel);
                resolved = ctx.RendererRegistry.Resolve(probeArgs);
                height = resolved.GetHeight(probeArgs);
            }

            var decorators = ResolveTopDecorators(ctx, property);
            var leftPad = Mathf.Max(0f, depth) * DepthIndent;
            var rightReserved = ResetWidth + ResetSpacing;

            if (decorators.TopSpacePx > 0f)
                GUILayout.Space(decorators.TopSpacePx);

            if (decorators.DrawDivider)
            {
                var blockRect = EditorGUILayout.GetControlRect(false, DividerTopPadding + DividerLineHeight + DividerBottomPadding);
                var dividerRect = new Rect(
                    blockRect.x + leftPad,
                    blockRect.y + DividerTopPadding,
                    Mathf.Max(0f, blockRect.width - leftPad - rightReserved),
                    DividerLineHeight);
                EditorGUI.DrawRect(dividerRect, new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.28f : 0.18f));
            }

            var totalRect = EditorGUILayout.GetControlRect(false, height);
            var layout = PropertyRowLayoutCalculator.Build(totalRect, height, depth);
            var args = new PropertyRendererArgs(
                layout.MainRect,
                ctx.MaterialEditor,
                ctx.Material,
                property,
                label,
                ctx.Styles.MiniGray,
                layout,
                propertyModel);
            var previousValue = new PropertyValueSnapshot(property,
                MaterialGUIPropertyValidationRegistry.HasValidators(ctx.Material.shader, property.name));

            PropertyAnimationContextMenu.HandleContextClick(ctx, property, label, layout.MainRect);

            using (new EditorGUIWideModeScope(true, layout.LabelRect.width))
            {
                DrawChangedMarker(layout.MainRect, propertyModel);
                var propertyScope = BeginMaterialPropertyScope(ctx, layout.MainRect, property);
                var animatedScope = BeginAnimatedPropertyScope(ctx, layout.MainRect, property);
                try
                {
                    EditorGUI.BeginChangeCheck();
                    bool rendererChanged;
                    try { resolved.OnGUI(args); }
                    finally { rendererChanged = EditorGUI.EndChangeCheck(); }

                    var valueChanged = !previousValue.Matches(property);
                    if (valueChanged || rendererChanged)
                    {
                        var accepted = MaterialGUIPropertyValidationRegistry.Apply(ctx, property, label, layout);
                        if (!accepted)
                        {
                            previousValue.Restore(property);
                            GUI.changed = true;
                            ctx.MaterialEditor?.Repaint();
                        }
                        else if (!previousValue.Matches(property) && !RegisteredPropertyChanges.Contains(property))
                        {
                            RegisterPropertyValueChange(ctx.MaterialEditor, property, $"Change {label}");
                        }
                    }
                }
                finally
                {
                    EndAnimatedPropertyScope(ctx, animatedScope);
                    EndMaterialPropertyScope(ctx, propertyScope);
                }
            }

            DrawResetButton(layout.ResetRect, () => ResetPropertyToShaderDefault(ctx, property));
        }

        internal static void RegisterPropertyValueChange(MaterialEditor? editor, MaterialProperty property, string undoName)
        {
            if (property == null) return;
            if (editor != null)
            {
                try
                {
                    editor.RegisterPropertyChangeUndo(string.IsNullOrWhiteSpace(undoName)
                        ? $"Change {property.name}"
                        : undoName);
                }
                catch (System.Exception ex)
                {
                    MaterialGUIInternalDiagnostics.WarnOnce(
                        "property-change-undo:" + property.name + ":" + ex.GetType().FullName,
                        "Failed to register property change undo for '" + property.name + "': " + ex.Message);
                }
            }

            RegisteredPropertyChanges.Add(property);
            GUI.changed = true;
        }

        private static PropertyRowDecorators ResolveTopDecorators(EditorContext ctx, MaterialProperty property)
        {
            var topSpacePx = 0f;
            var drawDivider = false;

            var shader = ctx.Material != null ? ctx.Material.shader : null;
            if (shader != null)
            {
                if (ShaderPropertyAttributeCache.TryGetTopSpacePx(shader, property.name, out var px))
                    topSpacePx = Mathf.Max(0f, px);
                drawDivider = ShaderPropertyAttributeCache.HasDivider(shader, property.name);
            }

            return new PropertyRowDecorators(topSpacePx, drawDivider);
        }

        private static void DrawResetButton(Rect resetRect, System.Action onClick)
        {
            GUI.Box(resetRect, GUIContent.none, EditorStyles.miniButton);
            var icon = EditorGUIUtility.IconContent("d_RotateTool");
            var iconRect = new Rect(
                resetRect.x + (resetRect.width - 14f) * 0.5f,
                resetRect.y + (resetRect.height - 14f) * 0.5f,
                14f,
                14f);
            GUI.Label(iconRect, icon);
            EditorGUIUtility.AddCursorRect(resetRect, MouseCursor.Link);
            if (GUI.Button(resetRect, GUIContent.none, GUIStyle.none))
                onClick?.Invoke();
        }

        private static bool BeginMaterialPropertyScope(EditorContext ctx, Rect rect, MaterialProperty property)
        {
            if (ctx?.MaterialEditor == null || property == null) return false;
            try
            {
                MaterialEditor.BeginProperty(rect, property);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void EndMaterialPropertyScope(EditorContext ctx, bool active)
        {
            if (!active || ctx?.MaterialEditor == null) return;
            try
            {
                MaterialEditor.EndProperty();
            }
            catch (System.Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "property-scope.end:" + ex.GetType().FullName,
                    "Failed to end Unity material property scope: " + ex.Message);
            }
        }

        private static bool BeginAnimatedPropertyScope(EditorContext ctx, Rect rect, MaterialProperty property)
        {
            if (ctx?.MaterialEditor == null || property == null) return false;
            try
            {
                ctx.MaterialEditor.BeginAnimatedCheck(rect, property);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void EndAnimatedPropertyScope(EditorContext ctx, bool active)
        {
            if (!active || ctx?.MaterialEditor == null) return;
            try
            {
                ctx.MaterialEditor.EndAnimatedCheck();
            }
            catch (System.Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation-scope.end:" + ex.GetType().FullName,
                    "Failed to end Unity animated material property scope: " + ex.Message);
            }
        }

        private static ShaderPropertyModel? FindModel(EditorContext ctx, MaterialProperty property)
        {
            return ctx?.Model != null && property != null &&
                   ctx.Model.TryGetProperty(property.name, out var model) ? model : null;
        }

        private static void DrawChangedMarker(Rect mainRect, ShaderPropertyModel? model)
        {
            if (model == null || !model.Changed) return;

            var marker = new Rect(mainRect.x - 3f, mainRect.y + 2f, 2f, Mathf.Max(0f, mainRect.height - 4f));
            EditorGUI.DrawRect(marker, EditorGUIUtility.isProSkin
                ? new Color(0.45f, 0.68f, 1f, 0.9f)
                : new Color(0.16f, 0.38f, 0.82f, 0.9f));
        }

        public static void ResetPropertyToShaderDefault(EditorContext ctx, MaterialProperty property)
        {
            ResetPropertiesToShaderDefaults(ctx, new[] { property });
        }

        public static void ResetPropertiesToShaderDefaults(EditorContext ctx, IEnumerable<MaterialProperty> properties)
        {
            if (ctx == null || properties == null) return;
            var targets = GetTargetMaterials(ctx);
            if (targets.Length == 0) return;
            var propertyList = properties.Where(p => p != null).Distinct().ToArray();
            if (propertyList.Length == 0) return;

            var objects = targets
                .Where(static m => m != null)
                .Cast<Object>()
                .ToArray();
            if (objects.Length == 0) return;

            Undo.RecordObjects(objects, "Reset Properties");
            for (var i = 0; i < targets.Length; i++)
            {
                var mat = targets[i];
                if (mat == null || mat.shader == null) continue;
                var defaultMat = ShaderDefaultMaterialCache.Get(mat.shader);
                if (defaultMat == null) continue;
                foreach (var property in propertyList)
                    ApplyDefaultValue(mat, defaultMat, property);
                MaterialEditor.ApplyMaterialPropertyDrawers(mat);
                EditorUtility.SetDirty(mat);
            }

            try
            {
                ctx.MaterialEditor?.PropertiesChanged();
            }
            catch (System.Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "reset.properties-changed:" + ex.GetType().FullName,
                    "Failed to refresh material editor after reset: " + ex.Message);
            }

            GUI.changed = true;
            ctx.MaterialEditor?.Repaint();
        }

        private static void ApplyDefaultValue(Material target, Material defaultMat, MaterialProperty property)
        {
            if (target == null || defaultMat == null || property == null) return;

            var has = defaultMat.HasProperty(property.name) && target.HasProperty(property.name);

            switch (property.type)
            {
                case MaterialProperty.PropType.Int:
                    target.SetInteger(property.name, has ? defaultMat.GetInteger(property.name) : 0);
                    break;
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    target.SetFloat(property.name, has ? defaultMat.GetFloat(property.name) : 0f);
                    break;
                case MaterialProperty.PropType.Color:
                    target.SetColor(property.name, has ? defaultMat.GetColor(property.name) : Color.white);
                    break;
                case MaterialProperty.PropType.Vector:
                    target.SetVector(property.name, has ? defaultMat.GetVector(property.name) : Vector4.zero);
                    break;
                case MaterialProperty.PropType.Texture:
                    target.SetTexture(property.name, has ? defaultMat.GetTexture(property.name) : null);
                    if (has)
                    {
                        target.SetTextureScale(property.name, defaultMat.GetTextureScale(property.name));
                        target.SetTextureOffset(property.name, defaultMat.GetTextureOffset(property.name));
                    }
                    else
                    {
                        target.SetTextureScale(property.name, Vector2.one);
                        target.SetTextureOffset(property.name, Vector2.zero);
                    }

                    break;
            }
        }

        private static Material[] GetTargetMaterials(EditorContext ctx)
        {
            if (ctx?.Targets != null && ctx.Targets.Count > 0)
                return ctx.Targets.Where(static m => m != null).ToArray();
            if (ctx?.Material != null)
                return new[] { ctx.Material };
            return System.Array.Empty<Material>();
        }

        private readonly struct PropertyValueSnapshot
        {
            private readonly bool _mixed;
            private readonly MaterialProperty[]? _targetValues;
            private readonly string[][]? _targetKeywords;
            private readonly MaterialProperty.PropType _type;
            private readonly int _intValue;
            private readonly float _floatValue;
            private readonly Color _colorValue;
            private readonly Vector4 _vectorValue;
            private readonly Texture? _textureValue;
            private readonly Vector4 _textureScaleOffset;

            public PropertyValueSnapshot(MaterialProperty property) : this(property, true) { }

            public PropertyValueSnapshot(MaterialProperty property, bool captureTargets)
            {
                _mixed = property.hasMixedValue;
                _targetValues = null;
                _targetKeywords = null;
                if (captureTargets)
                {
                    var targets = property.targets;
                    _targetValues = new MaterialProperty[targets.Length];
                    _targetKeywords = new string[targets.Length][];
                    for (var i = 0; i < targets.Length; i++)
                    {
                        _targetValues[i] = MaterialEditor.GetMaterialProperty(new[] { targets[i] }, property.name);
                        _targetKeywords[i] = targets[i] is Material material ? material.shaderKeywords : System.Array.Empty<string>();
                    }
                }
                _type = property.type;
                _intValue = property.type == MaterialProperty.PropType.Int ? property.intValue : 0;
                _floatValue = 0f;
                _colorValue = default;
                _vectorValue = default;
                _textureValue = null;
                _textureScaleOffset = default;

                switch (property.type)
                {
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        _floatValue = property.floatValue;
                        break;
                    case MaterialProperty.PropType.Color:
                        _colorValue = property.colorValue;
                        break;
                    case MaterialProperty.PropType.Vector:
                        _vectorValue = property.vectorValue;
                        break;
                    case MaterialProperty.PropType.Texture:
                        _textureValue = property.textureValue;
                        _textureScaleOffset = property.textureScaleAndOffset;
                        break;
                }
            }

            public void Restore(MaterialProperty property)
            {
                if (_targetValues != null)
                {
                    // Never write the representative value back across a mixed selection.
                    for (var i = 0; i < _targetValues.Length; i++)
                    {
                        var saved = _targetValues[i];
                        if (saved.targets[0] is not Material target || target == null) continue;
                        switch (_type)
                        {
                            case MaterialProperty.PropType.Int: target.SetInteger(saved.name, saved.intValue); break;
                            case MaterialProperty.PropType.Float:
                            case MaterialProperty.PropType.Range: target.SetFloat(saved.name, saved.floatValue); break;
                            case MaterialProperty.PropType.Color: target.SetColor(saved.name, saved.colorValue); break;
                            case MaterialProperty.PropType.Vector: target.SetVector(saved.name, saved.vectorValue); break;
                            case MaterialProperty.PropType.Texture:
                                target.SetTexture(saved.name, saved.textureValue);
                                var st = saved.textureScaleAndOffset;
                                target.SetTextureScale(saved.name, new Vector2(st.x, st.y));
                                target.SetTextureOffset(saved.name, new Vector2(st.z, st.w));
                                break;
                        }
                        target.shaderKeywords = _targetKeywords![i];
                        EditorUtility.SetDirty(target);
                    }
                    return;
                }
                switch (_type)
                {
                    case MaterialProperty.PropType.Int:
                        property.intValue = _intValue;
                        break;
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        property.floatValue = _floatValue;
                        break;
                    case MaterialProperty.PropType.Color:
                        property.colorValue = _colorValue;
                        break;
                    case MaterialProperty.PropType.Vector:
                        property.vectorValue = _vectorValue;
                        break;
                    case MaterialProperty.PropType.Texture:
                        property.textureValue = _textureValue;
                        property.textureScaleAndOffset = _textureScaleOffset;
                        break;
                }
            }

            public bool Matches(MaterialProperty property)
            {
                if (property == null || property.type != _type || property.hasMixedValue != _mixed) return false;

                switch (_type)
                {
                    case MaterialProperty.PropType.Int:
                        return property.intValue == _intValue;
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        return Mathf.Approximately(property.floatValue, _floatValue);
                    case MaterialProperty.PropType.Color:
                        return property.colorValue == _colorValue;
                    case MaterialProperty.PropType.Vector:
                        return property.vectorValue == _vectorValue;
                    case MaterialProperty.PropType.Texture:
                        return property.textureValue == _textureValue &&
                               property.textureScaleAndOffset == _textureScaleOffset;
                    default:
                        return true;
                }
            }
        }
    }
}


