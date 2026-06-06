#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityRenderer = UnityEngine.Renderer;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    internal static class PropertyAnimationContextMenu
    {
        private static readonly Type? AnimationWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");
        private static readonly Type? MaterialAnimationUtilityType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.MaterialAnimationUtility");
        private static readonly MethodInfo? MaterialPropertyToModificationsMethod =
            MaterialAnimationUtilityType?.GetMethod(
                "MaterialPropertyToPropertyModifications",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(MaterialProperty), typeof(UnityRenderer) },
                null);
        private static readonly PropertyInfo? RendererForAnimationModeProperty =
            typeof(MaterialEditor).GetProperty("rendererForAnimationMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo? InAnimationRecordingMethod = typeof(AnimationMode).GetMethod(
            "InAnimationRecording",
            BindingFlags.NonPublic | BindingFlags.Static);
        private static CopiedPropertyValue? s_CopiedValue;

        internal static void HandleContextClick(EditorContext ctx, MaterialProperty property, string label, Rect rect)
        {
            var e = Event.current;
            if (e == null || !rect.Contains(e.mousePosition))
                return;

            if (e.type != EventType.ContextClick && (e.type != EventType.MouseDown || e.button != 1))
                return;
            if (!IsAnimationRecording())
                return;

            Show(ctx, property, label);
            e.Use();
        }

        private static void Show(EditorContext ctx, MaterialProperty property, string label)
        {
            var menu = new GenericMenu();
            var context = BuildContext(ctx, property);
            var hasControl = context.Control != null;
            var hasModifications = context.Modifications.Length > 0;
            var hasCurve = hasControl && hasModifications && InvokeBool(context.Control, "CurveExists", context.Modifications);
            var hasKey = hasControl && hasModifications && InvokeBool(context.Control, "KeyExists", context.Modifications);

            AddControlItem(menu, hasControl && hasModifications, "Add Key", context.Control, "AddKey", context.Modifications);
            AddControlItem(menu, hasKey, "Remove Key", context.Control, "RemoveKey", context.Modifications);
            AddControlItem(menu, hasCurve, "Remove All Keys", context.Control, "RemoveCurve", context.Modifications);

            menu.AddSeparator(string.Empty);
            AddControlItem(menu, hasControl, "Key All Modified", context.Control, "AddCandidateKeys", null);
            AddControlItem(menu, hasControl, "Key All Animated", context.Control, "AddAnimatedKeys", null);

            menu.AddSeparator(string.Empty);
            AddControlItem(menu, hasCurve, "Go to Previous Key", context.Control, "GoToPreviousKeyframe", context.Modifications);
            AddControlItem(menu, hasCurve, "Go to Next Key", context.Control, "GoToNextKeyframe", context.Modifications);

            menu.AddSeparator(string.Empty);
            AddDefaultPropertyItems(menu, ctx, property);

            if (!hasControl || !hasModifications)
            {
                menu.AddSeparator(string.Empty);
                var reason = !hasControl
                    ? "Animation Window target required"
                    : "No animated renderer uses this material";
                menu.AddDisabledItem(new GUIContent(reason));
            }

            menu.ShowAsContext();
        }

        private static bool IsAnimationRecording()
        {
            if (!AnimationMode.InAnimationMode()) return false;
            if (InAnimationRecordingMethod == null)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation.recording.missing",
                    "Animation key menu recording check is unavailable because AnimationMode.InAnimationRecording was not found.");
                return false;
            }

            try
            {
                return InAnimationRecordingMethod.Invoke(null, null) is true;
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation.recording.invoke:" + ex.GetType().FullName,
                    $"Animation key menu recording check failed: {ex.Message}");
                return false;
            }
        }

        private static void AddDefaultPropertyItems(GenericMenu menu, EditorContext ctx, MaterialProperty property)
        {
            menu.AddItem(new GUIContent("Copy"), false, () => CopyPropertyValue(property));

            if (CanPaste(property))
                menu.AddItem(new GUIContent("Paste"), false, () => PastePropertyValue(ctx, property));
            else
                menu.AddDisabledItem(new GUIContent("Paste"));

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Reset"), false, () => PropertyRowHost.ResetPropertyToShaderDefault(ctx, property));

            menu.AddSeparator(string.Empty);
            menu.AddDisabledItem(new GUIContent("Lock in children"));
        }

        private static void CopyPropertyValue(MaterialProperty property)
        {
            s_CopiedValue = CopiedPropertyValue.Capture(property);
            EditorGUIUtility.systemCopyBuffer = s_CopiedValue.Value.DisplayValue;
        }

        private static bool CanPaste(MaterialProperty property)
        {
            return s_CopiedValue.HasValue && s_CopiedValue.Value.Type == property.type;
        }

        private static void PastePropertyValue(EditorContext ctx, MaterialProperty property)
        {
            if (!CanPaste(property)) return;
            var value = s_CopiedValue!.Value;
            PropertyRowHost.RegisterPropertyValueChange(ctx.MaterialEditor, property, $"Paste {property.displayName}");

            switch (property.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    property.floatValue = value.FloatValue;
                    break;
                case MaterialProperty.PropType.Color:
                    property.colorValue = value.ColorValue;
                    break;
                case MaterialProperty.PropType.Vector:
                    property.vectorValue = value.VectorValue;
                    break;
                case MaterialProperty.PropType.Texture:
                    property.textureValue = value.TextureValue;
                    property.textureScaleAndOffset = value.TextureScaleOffset;
                    break;
            }

            GUI.changed = true;
            ctx.MaterialEditor?.PropertiesChanged();
            ctx.MaterialEditor?.Repaint();
        }

        private static AnimationContext BuildContext(EditorContext ctx, MaterialProperty property)
        {
            var window = FindAnimationWindow();
            var state = GetValue(window, "state");
            var control = GetValue(state, "controlInterface");
            var renderers = FindTargetRenderers(ctx, property, state);
            var modifications = BuildModifications(property, renderers);
            return new AnimationContext(control, modifications);
        }

        private static EditorWindow? FindAnimationWindow()
        {
            if (AnimationWindowType == null)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation.window.missing",
                    "Animation key menu integration is unavailable because UnityEditor.AnimationWindow was not found.");
                return null;
            }

            var focused = EditorWindow.focusedWindow;
            if (focused != null && AnimationWindowType.IsInstanceOfType(focused))
                return focused;

            var windows = Resources.FindObjectsOfTypeAll(AnimationWindowType);
            if (windows == null || windows.Length == 0)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation.window.none",
                    "Animation key menu integration did not find an open Animation Window.");
                return null;
            }
            for (var i = 0; i < windows.Length; i++)
            {
                if (windows[i] is EditorWindow window)
                    return window;
            }

            return null;
        }

        private static UnityRenderer[] FindTargetRenderers(EditorContext ctx, MaterialProperty property, object? animationState)
        {
            var result = new List<UnityRenderer>();
            var materials = property.targets?.OfType<Material>().Where(static m => m != null).Distinct().ToArray()
                            ?? Array.Empty<Material>();
            if (materials.Length == 0 && ctx.Material != null)
                materials = new[] { ctx.Material };

            var direct = GetRendererForAnimationMode(ctx.MaterialEditor);
            AddRendererIfUsesMaterial(result, direct, materials);

            AddMatchingRenderers(result, GetValue(animationState, "activeGameObject") as GameObject, materials, false);
            AddMatchingRenderers(result, GetValue(animationState, "activeRootGameObject") as GameObject, materials, true);

            var selected = Selection.gameObjects;
            if (selected != null)
            {
                for (var i = 0; i < selected.Length; i++)
                    AddMatchingRenderers(result, selected[i], materials, true);
            }

            return result.Where(static r => r != null).Distinct().ToArray();
        }

        private static UnityRenderer? GetRendererForAnimationMode(MaterialEditor? editor)
        {
            if (editor == null) return null;
            if (RendererForAnimationModeProperty == null)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation.rendererForAnimationMode.missing",
                    "Animation key menu renderer lookup is unavailable because MaterialEditor.rendererForAnimationMode was not found.");
                return null;
            }
            try
            {
                return RendererForAnimationModeProperty.GetValue(editor, null) as UnityRenderer;
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation.rendererForAnimationMode.invoke:" + ex.GetType().FullName,
                    $"Animation key menu renderer lookup failed: {ex.Message}");
                return null;
            }
        }

        private static void AddMatchingRenderers(List<UnityRenderer> result, GameObject? root, Material[] materials, bool includeChildren)
        {
            if (root == null) return;

            if (includeChildren)
            {
                var renderers = root.GetComponentsInChildren<UnityRenderer>(true);
                for (var i = 0; i < renderers.Length; i++)
                    AddRendererIfUsesMaterial(result, renderers[i], materials);
            }
            else
            {
                var renderers = root.GetComponents<UnityRenderer>();
                for (var i = 0; i < renderers.Length; i++)
                    AddRendererIfUsesMaterial(result, renderers[i], materials);
            }
        }

        private static void AddRendererIfUsesMaterial(List<UnityRenderer> result, UnityRenderer? renderer, Material[] materials)
        {
            if (renderer == null || materials.Length == 0) return;
            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0) return;

            for (var i = 0; i < shared.Length; i++)
            {
                var material = shared[i];
                if (material == null) continue;

                for (var j = 0; j < materials.Length; j++)
                {
                    if (!ReferenceEquals(material, materials[j])) continue;
                    if (!result.Contains(renderer))
                        result.Add(renderer);
                    return;
                }
            }
        }

        private static PropertyModification[] BuildModifications(MaterialProperty property, UnityRenderer[] renderers)
        {
            if (MaterialPropertyToModificationsMethod == null)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "animation.modifications.missing",
                    "Animation key menu modification lookup is unavailable because MaterialAnimationUtility.MaterialPropertyToPropertyModifications was not found.");
                return Array.Empty<PropertyModification>();
            }

            if (renderers.Length == 0)
                return Array.Empty<PropertyModification>();

            var result = new List<PropertyModification>();
            for (var i = 0; i < renderers.Length; i++)
            {
                try
                {
                    if (MaterialPropertyToModificationsMethod.Invoke(null, new object[] { property, renderers[i] }) is not PropertyModification[] mods)
                        continue;
                    for (var j = 0; j < mods.Length; j++)
                    {
                        if (mods[j] != null)
                            result.Add(mods[j]);
                    }
                }
                catch (Exception ex)
                {
                    MaterialGUIInternalDiagnostics.WarnOnce(
                        "animation.modifications.invoke:" + ex.GetType().FullName,
                        $"Animation key menu modification lookup failed: {ex.Message}");
                }
            }

            return result.ToArray();
        }

        private static void AddControlItem(
            GenericMenu menu,
            bool enabled,
            string label,
            object? control,
            string methodName,
            PropertyModification[]? modifications)
        {
            var content = new GUIContent(label);
            if (!enabled || control == null)
            {
                menu.AddDisabledItem(content);
                return;
            }

            menu.AddItem(content, false, () => InvokeControl(control, methodName, modifications));
        }

        private static bool InvokeBool(object? control, string methodName, PropertyModification[] modifications)
        {
            if (control == null) return false;
            try
            {
                var method = control.GetType().GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(PropertyModification[]) },
                    null);
                return method?.Invoke(control, new object[] { modifications }) is true;
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    $"animation.control.bool:{methodName}:{ex.GetType().FullName}",
                    $"Animation key menu command state '{methodName}' failed: {ex.Message}");
                return false;
            }
        }

        private static void InvokeControl(object control, string methodName, PropertyModification[]? modifications)
        {
            try
            {
                MethodInfo? method;
                object[] args;
                if (modifications == null)
                {
                    method = control.GetType().GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        Type.EmptyTypes,
                        null);
                    args = Array.Empty<object>();
                }
                else
                {
                    method = control.GetType().GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new[] { typeof(PropertyModification[]) },
                        null);
                    args = new object[] { modifications };
                }

                method?.Invoke(control, args);
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    $"animation.control.invoke:{methodName}:{ex.GetType().FullName}",
                    $"Animation key menu command '{methodName}' failed: {ex.Message}");
            }
        }

        private static object? GetValue(object? target, string name)
        {
            if (target == null) return null;
            var type = target.GetType();

            try
            {
                var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                    return property.GetValue(target, null);

                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return field?.GetValue(target);
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    $"animation.value:{name}:{ex.GetType().FullName}",
                    $"Animation key menu internal value lookup '{name}' failed: {ex.Message}");
                return null;
            }
        }

        private readonly struct AnimationContext
        {
            public readonly object? Control;
            public readonly PropertyModification[] Modifications;

            public AnimationContext(object? control, PropertyModification[] modifications)
            {
                Control = control;
                Modifications = modifications ?? Array.Empty<PropertyModification>();
            }
        }

        private readonly struct CopiedPropertyValue
        {
            public readonly MaterialProperty.PropType Type;
            public readonly float FloatValue;
            public readonly Color ColorValue;
            public readonly Vector4 VectorValue;
            public readonly Texture? TextureValue;
            public readonly Vector4 TextureScaleOffset;
            public readonly string DisplayValue;

            private CopiedPropertyValue(
                MaterialProperty.PropType type,
                float floatValue,
                Color colorValue,
                Vector4 vectorValue,
                Texture? textureValue,
                Vector4 textureScaleOffset,
                string displayValue)
            {
                Type = type;
                FloatValue = floatValue;
                ColorValue = colorValue;
                VectorValue = vectorValue;
                TextureValue = textureValue;
                TextureScaleOffset = textureScaleOffset;
                DisplayValue = displayValue;
            }

            public static CopiedPropertyValue Capture(MaterialProperty property)
            {
                switch (property.type)
                {
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        return new CopiedPropertyValue(
                            property.type,
                            property.floatValue,
                            default,
                            default,
                            null,
                            default,
                            property.floatValue.ToString("R", CultureInfo.InvariantCulture));
                    case MaterialProperty.PropType.Color:
                        return new CopiedPropertyValue(
                            property.type,
                            0f,
                            property.colorValue,
                            default,
                            null,
                            default,
                            property.colorValue.ToString());
                    case MaterialProperty.PropType.Vector:
                        return new CopiedPropertyValue(
                            property.type,
                            0f,
                            default,
                            property.vectorValue,
                            null,
                            default,
                            property.vectorValue.ToString());
                    case MaterialProperty.PropType.Texture:
                        return new CopiedPropertyValue(
                            property.type,
                            0f,
                            default,
                            default,
                            property.textureValue,
                            property.textureScaleAndOffset,
                            property.textureValue != null ? property.textureValue.name : "None");
                    default:
                        return new CopiedPropertyValue(property.type, 0f, default, default, null, default, string.Empty);
                }
            }
        }
    }
}


