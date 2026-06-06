#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class EnumPropertyRenderer : IMaterialPropertyRenderer
    {
        public bool CanRender(in PropertyRendererArgs args)
        {
            if (args.Property.type != MaterialProperty.PropType.Float) return false;
            var mat = args.Property.targets != null && args.Property.targets.Length > 0 ? args.Property.targets[0] as Material : null;
            var shader = mat != null ? mat.shader : null;
            return ShaderPropertyAttributeCache.TryGetEnum(shader, args.Property.name, out _);
        }

        public float GetHeight(in PropertyRendererArgs args)
            => EditorGUIUtility.singleLineHeight;

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);

            var mat = args.Material ?? (args.Property.targets != null && args.Property.targets.Length > 0 ? args.Property.targets[0] as Material : null);
            var shader = mat != null ? mat.shader : null;

            if (!ShaderPropertyAttributeCache.TryGetEnum(shader, args.Property.name, out var info))
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.FloatField(fieldRect, GUIContent.none, args.Property.floatValue);
                if (EditorGUI.EndChangeCheck()) args.SetFloatValue(v);
                return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
            }

            int curIdx = 0;
            var cur = args.Property.floatValue;
            float best = float.PositiveInfinity;
            for (int i = 0; i < info.values.Length; i++)
            {
                float d = Mathf.Abs(info.values[i] - cur);
                if (d < best)
                {
                    best = d;
                    curIdx = i;
                }
            }

            EditorGUI.BeginChangeCheck();
            var next = EditorGUI.Popup(fieldRect, curIdx, info.names);
            if (EditorGUI.EndChangeCheck())
                args.SetFloatValue(info.values[Mathf.Clamp(next, 0, info.values.Length - 1)], "Enum Change");

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }
    }
}


