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
            var previousValue = new PropertyValueSnapshot(property);

            PropertyAnimationContextMenu.HandleContextClick(ctx, property, label, layout.MainRect);

            using (new EditorGUIWideModeScope(true, layout.LabelRect.width))
            {
                DrawChangedMarker(layout.MainRect, propertyModel);
                var propertyScope = BeginMaterialPropertyScope(ctx, layout.MainRect, property);
                var animatedScope = BeginAnimatedPropertyScope(ctx, layout.MainRect, property);
                try
                {
                    EditorGUI.BeginChangeCheck();
                    resolved.OnGUI(args);
                    var rendererChanged = EditorGUI.EndChangeCheck();

                    var accepted = MaterialGUIPropertyValidationRegistry.Apply(ctx, property, label, layout);
                    if (!accepted)
                    {
                        previousValue.Restore(property);
                        GUI.changed = true;
                    }
                    else if ((rendererChanged || !previousValue.Matches(property)) &&
                             !previousValue.Matches(property) &&
                             !RegisteredPropertyChanges.Contains(property))
                    {
                        RegisterPropertyValueChange(ctx.MaterialEditor, property, $"Change {label}");
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
            var model = ctx?.Model;
            if (model == null || property == null) return null;

            var properties = model.Properties;
            for (var i = 0; i < properties.Count; i++)
            {
                var candidate = properties[i];
                if (candidate != null && candidate.Property == property)
                    return candidate;
            }

            return null;
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
            var targets = GetTargetMaterials(ctx);
            if (targets.Length == 0) return;

            var objects = targets
                .Where(static m => m != null)
                .Cast<Object>()
                .ToArray();
            if (objects.Length == 0) return;

            Undo.RecordObjects(objects, "Reset Property");

            var defaultsByShader = new Dictionary<Shader, Material>();
            try
            {
                for (var i = 0; i < targets.Length; i++)
                {
                    var mat = targets[i];
                    if (mat == null || mat.shader == null) continue;

                    if (!defaultsByShader.TryGetValue(mat.shader, out var defaultMat))
                    {
                        defaultMat = new Material(mat.shader);
                        defaultsByShader.Add(mat.shader, defaultMat);
                    }

                    ApplyDefaultValue(mat, defaultMat, property);
                    EditorUtility.SetDirty(mat);
                }
            }
            finally
            {
                foreach (var kv in defaultsByShader)
                {
                    if (kv.Value != null)
                        Object.DestroyImmediate(kv.Value);
                }
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

            var preview = FindFirstMaterial(targets) ?? ctx.Material;
            if (preview != null)
                SyncPropertyWrapperFromMaterial(property, preview);

            GUI.changed = true;
            ctx.MaterialEditor?.Repaint();
        }

        public static void ResetPropertiesToShaderDefaults(EditorContext ctx, IEnumerable<MaterialProperty> properties)
        {
            if (ctx == null || properties == null) return;
            foreach (var property in properties)
            {
                if (property == null) continue;
                ResetPropertyToShaderDefault(ctx, property);
            }
        }

        private static void ApplyDefaultValue(Material target, Material defaultMat, MaterialProperty property)
        {
            if (target == null || defaultMat == null || property == null) return;

            var has = defaultMat.HasProperty(property.name) && target.HasProperty(property.name);

            switch (property.type)
            {
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

        private static Material? FindFirstMaterial(Material[] targets)
        {
            if (targets == null || targets.Length == 0) return null;
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null) return targets[i];
            }
            return null;
        }

        private static void SyncPropertyWrapperFromMaterial(MaterialProperty property, Material material)
        {
            if (property == null || material == null) return;
            if (!material.HasProperty(property.name)) return;

            switch (property.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    property.floatValue = material.GetFloat(property.name);
                    break;
                case MaterialProperty.PropType.Color:
                    property.colorValue = material.GetColor(property.name);
                    break;
                case MaterialProperty.PropType.Vector:
                    property.vectorValue = material.GetVector(property.name);
                    break;
                case MaterialProperty.PropType.Texture:
                    property.textureValue = material.GetTexture(property.name);
                    var scale = material.GetTextureScale(property.name);
                    var offset = material.GetTextureOffset(property.name);
                    property.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                    break;
            }
        }

        private readonly struct PropertyValueSnapshot
        {
            private readonly MaterialProperty.PropType _type;
            private readonly float _floatValue;
            private readonly Color _colorValue;
            private readonly Vector4 _vectorValue;
            private readonly Texture? _textureValue;
            private readonly Vector4 _textureScaleOffset;

            public PropertyValueSnapshot(MaterialProperty property)
            {
                _type = property.type;
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
                switch (_type)
                {
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
                if (property == null || property.type != _type) return false;

                switch (_type)
                {
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


