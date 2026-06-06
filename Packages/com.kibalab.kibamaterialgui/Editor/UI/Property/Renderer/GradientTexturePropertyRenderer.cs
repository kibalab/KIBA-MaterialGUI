#nullable enable

using System;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using KIBA_.KIBAMaterialGUI.Editor.IO;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class GradientTexturePropertyRenderer : IMaterialPropertyRenderer
    {
        private const float KeyHandleW = 8f;
        private const float KeyHandleH = 12f;

        private static readonly int s_DragHash = "GradientKeyDrag".GetHashCode();
        private static readonly int s_DefaultGradientHash = ComputeGradientHash(CreateDefaultGradient());
        private static readonly Dictionary<int, int> s_LastGradientHashByMeta = new();
        private static int s_ActiveDragIndex = -1;
        private static float s_DragStartMouseX;
        private static float s_DragStartTime;
        private static float s_DragFieldXMin;
        private static float s_DragFieldWidth;

        private readonly GradientTexturePersistenceService _persistence = new();

        public static bool IsGradientCandidate(MaterialProperty property)
        {
            if (property.type != MaterialProperty.PropType.Texture) return false;

            Shader? shader = null;
            var targets = property.targets;
            if (targets != null)
            {
                for (var i = 0; i < targets.Length; i++)
                {
                    if (targets[i] is not Material mat || mat.shader == null) continue;
                    shader = mat.shader;
                    break;
                }
            }

            if (ShaderPropertyAttributeCache.HasUiHint(shader, property.name, ShaderPropertyAttributeCache.UiHintFlags.GradientTexture))
                return true;

            var name = property.name ?? string.Empty;

            return name.EndsWith("_gradient", StringComparison.OrdinalIgnoreCase) ||
                   name.EndsWith("_gradienttex", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanRender(in PropertyRendererArgs args)
        {
            return IsGradientCandidate(args.Property);
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            if (args.Material == null) return args.Position;

            var meta = _persistence.LoadOrCreateMetadata(args.Material, args.Property);
            var tex = _persistence.LoadOrCreateTexture(args.Material, args.Property, meta);
            meta.Gradient ??= new Gradient();

            var materialTexture = GetCurrentMaterialTexture(args.Material, args.Property.name, args.Property.textureValue);
            if (!ReferenceEquals(materialTexture, args.Property.textureValue))
                args.Property.textureValue = materialTexture;

            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);
            var gradient = meta.Gradient;

            HandleKeyDrag(fieldRect, gradient, meta, out var keyDragging, out var keyDragEnded);

            bool gradientChanged;
            using (var so = new SerializedObject(meta))
            {
                so.Update();
                var gradProp = so.FindProperty("Gradient");
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(fieldRect, gradProp, GUIContent.none);
                gradientChanged = EditorGUI.EndChangeCheck();
                if (gradientChanged)
                    so.ApplyModifiedProperties();
            }
            gradient = meta.Gradient;

            DrawKeyHandles(fieldRect, gradient.colorKeys);

            var metaId = meta.GetInstanceID();
            var gradientHash = ComputeGradientHash(gradient);
            var isManagedTexture = ReferenceEquals(materialTexture, tex);

            if (!isManagedTexture && !keyDragging && !gradientChanged)
            {
                var externalTex = materialTexture;
                if (IsResetLikeTexture(externalTex))
                {
                    if (gradientHash != s_DefaultGradientHash)
                    {
                        var defaultGradient = CreateDefaultGradient();
                        Undo.RecordObject(meta, "Reset Gradient");
                        meta.Gradient = defaultGradient;
                        gradientHash = ComputeGradientHash(defaultGradient);
                        EditorUtility.SetDirty(meta);
                        GradientTexturePersistenceService.BakeIntoTexture(meta, tex);
                    }
                    else
                    {
                        GradientTexturePersistenceService.BakePixelsOnly(meta, tex);
                    }

                    AssignGradientTexture(args, tex);
                    args.MaterialEditor?.PropertiesChanged();
                    args.MaterialEditor?.Repaint();
                    s_LastGradientHashByMeta[metaId] = gradientHash;
                }

                return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
            }

            if (keyDragging)
            {
                EditorUtility.SetDirty(meta);
                GradientTexturePersistenceService.BakePixelsOnly(meta, tex);
                AssignGradientTexture(args, tex);
                s_LastGradientHashByMeta[metaId] = gradientHash;
            }
            else if (keyDragEnded)
            {
                EditorUtility.SetDirty(meta);
                GradientTexturePersistenceService.BakePixelsOnly(meta, tex);
                EditorUtility.SetDirty(tex);
                AssignGradientTexture(args, tex);
                s_LastGradientHashByMeta[metaId] = gradientHash;
            }
            else if (gradientChanged)
            {
                GradientTexturePersistenceService.BakeIntoTexture(meta, tex);
                AssignGradientTexture(args, tex);
                s_LastGradientHashByMeta[metaId] = gradientHash;
            }
            else
            {
                var needsSync = args.Property.textureValue != tex;
                if (!s_LastGradientHashByMeta.TryGetValue(metaId, out var lastHash) || lastHash != gradientHash)
                    needsSync = true;

                if (needsSync)
                {
                    GradientTexturePersistenceService.BakePixelsOnly(meta, tex);
                    AssignGradientTexture(args, tex);
                    s_LastGradientHashByMeta[metaId] = gradientHash;
                }
            }

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }

        private static void AssignGradientTexture(in PropertyRendererArgs args, Texture2D tex)
        {
            if (args.Material == null) return;
            args.RegisterPropertyValueChange("Change Gradient Texture");
            GradientTexturePersistenceService.AssignTextureToMaterial(args.Material, args.Property, tex);
        }

        private static void HandleKeyDrag(
            Rect fieldRect,
            Gradient gradient,
            GradientTextureMetadata meta,
            out bool dragging,
            out bool dragEnded)
        {
            dragging = false;
            dragEnded = false;

            var keys = gradient.colorKeys;
            if (keys == null || keys.Length == 0) return;

            var controlId = GUIUtility.GetControlID(s_DragHash, FocusType.Passive, fieldRect);
            var e = Event.current;

            switch (e.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (e.button != 0) break;
                    for (var i = 0; i < keys.Length; i++)
                    {
                        if (!GetKeyHandleRect(fieldRect, keys[i].time).Contains(e.mousePosition)) continue;

                        Undo.RegisterCompleteObjectUndo(meta, "Move Gradient Key");
                        GUIUtility.hotControl = controlId;
                        GUIUtility.keyboardControl = 0;
                        s_ActiveDragIndex = i;
                        s_DragStartMouseX = e.mousePosition.x;
                        s_DragStartTime = keys[i].time;
                        s_DragFieldXMin = fieldRect.xMin;
                        s_DragFieldWidth = fieldRect.width;
                        e.Use();
                        break;
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlId || s_ActiveDragIndex < 0 || s_ActiveDragIndex >= keys.Length) break;

                    var newTime = Mathf.Clamp01(s_DragStartTime + (e.mousePosition.x - s_DragStartMouseX) / s_DragFieldWidth);
                    keys[s_ActiveDragIndex] = new GradientColorKey(keys[s_ActiveDragIndex].color, newTime);
                    gradient.colorKeys = keys;
                    GUI.changed = true;
                    dragging = true;
                    e.Use();
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl != controlId) break;
                    GUIUtility.hotControl = 0;
                    s_ActiveDragIndex = -1;
                    dragEnded = true;
                    e.Use();
                    break;
            }
        }

        private static void DrawKeyHandles(Rect fieldRect, GradientColorKey[] keys)
        {
            if (keys == null || keys.Length == 0) return;

            var isRepaint = Event.current.type == EventType.Repaint;

            for (var i = 0; i < keys.Length; i++)
            {
                var handleRect = GetKeyHandleRect(fieldRect, keys[i].time);

                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeHorizontal);

                if (!isRepaint) continue;

                var isDragging = s_ActiveDragIndex == i;
                var borderColor = isDragging
                    ? Color.white
                    : (EditorGUIUtility.isProSkin
                        ? new Color(0.13f, 0.13f, 0.13f, 1f)
                        : new Color(0.45f, 0.45f, 0.45f, 1f));

                EditorGUI.DrawRect(handleRect, borderColor);

                var fillRect = new Rect(handleRect.x + 1f, handleRect.y + 1f, handleRect.width - 2f, handleRect.height - 2f);
                EditorGUI.DrawRect(fillRect, keys[i].color);
            }
        }

        private static Rect GetKeyHandleRect(Rect fieldRect, float keyTime)
        {
            var centerX = Mathf.Lerp(fieldRect.xMin, fieldRect.xMax, Mathf.Clamp01(keyTime));
            var x = centerX - KeyHandleW * 0.5f;
            var y = fieldRect.y + (fieldRect.height - KeyHandleH) * 0.5f;
            return new Rect(x, y, KeyHandleW, KeyHandleH);
        }

        private static int ComputeGradientHash(Gradient gradient)
        {
            unchecked
            {
                var hash = 17;
                if (gradient == null) return hash;

                var colorKeys = gradient.colorKeys;
                hash = hash * 31 + colorKeys.Length;
                for (var i = 0; i < colorKeys.Length; i++)
                {
                    var key = colorKeys[i];
                    hash = hash * 31 + key.time.GetHashCode();
                    hash = hash * 31 + key.color.GetHashCode();
                }

                var alphaKeys = gradient.alphaKeys;
                hash = hash * 31 + alphaKeys.Length;
                for (var i = 0; i < alphaKeys.Length; i++)
                {
                    var key = alphaKeys[i];
                    hash = hash * 31 + key.time.GetHashCode();
                    hash = hash * 31 + key.alpha.GetHashCode();
                }

                hash = hash * 31 + (int)gradient.mode;
                return hash;
            }
        }

        private static Gradient CreateDefaultGradient()
        {
            return new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(Color.black, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            };
        }

        private static bool IsResetLikeTexture(Texture? texture)
        {
            if (texture == null) return true;
            if (ReferenceEquals(texture, Texture2D.whiteTexture)) return true;

            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path))
                return true;

            var name = texture.name ?? string.Empty;
            if (name.Equals("white", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.IndexOf("default-white", StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return false;
        }

        private static Texture? GetCurrentMaterialTexture(Material material, string propertyName, Texture? fallback)
        {
            if (material == null || string.IsNullOrEmpty(propertyName)) return fallback;
            if (!material.HasProperty(propertyName)) return fallback;
            return material.GetTexture(propertyName);
        }
    }
}


