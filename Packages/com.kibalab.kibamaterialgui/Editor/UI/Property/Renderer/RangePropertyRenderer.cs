#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class RangePropertyRenderer : IMaterialPropertyRenderer
    {
        private const float ValueFieldWidth = 54f;
        private const float ValueFieldGap = 4f;

        public bool CanRender(in PropertyRendererArgs args)
        {
            return args.Property.type == MaterialProperty.PropType.Range;
        }

        public static bool IsFlexibleCandidate(MaterialProperty property)
        {
            if (property.type != MaterialProperty.PropType.Range &&
                property.type != MaterialProperty.PropType.Float)
                return false;

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

            if (ShaderPropertyAttributeCache.HasUiHint(shader, property.name, ShaderPropertyAttributeCache.UiHintFlags.FlexibleRange))
                return true;

            return false;
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);

            var hasUnit = FieldUnitHelper.TryGetUnit(args, out var unit);
            var flexible = IsFlexibleCandidate(args.Property);

            var lineRect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            var sliderWidth = Mathf.Max(0f, lineRect.width - ValueFieldWidth - ValueFieldGap);
            var sliderRect = new Rect(lineRect.x, lineRect.y, sliderWidth, lineRect.height);
            var valueRect = new Rect(sliderRect.xMax + ValueFieldGap, lineRect.y, ValueFieldWidth, lineRect.height);
            var current = args.Property.floatValue;
            var sliderValue = Mathf.Clamp(current, args.Property.rangeLimits.x, args.Property.rangeLimits.y);

            var prevMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = args.Property.hasMixedValue;

            EditorGUI.BeginChangeCheck();
            var sliderNext = GUI.HorizontalSlider(sliderRect, sliderValue, args.Property.rangeLimits.x, args.Property.rangeLimits.y);
            var sliderChanged = EditorGUI.EndChangeCheck();

            EditorGUI.BeginChangeCheck();
            var fieldNext = EditorGUI.FloatField(valueRect, current);
            var fieldChanged = EditorGUI.EndChangeCheck();

            EditorGUI.showMixedValue = prevMixed;

            if (sliderChanged || fieldChanged)
            {
                var next = sliderChanged ? sliderNext : fieldNext;
                if (fieldChanged)
                    next = fieldNext;
                if (!flexible)
                    next = Mathf.Clamp(next, args.Property.rangeLimits.x, args.Property.rangeLimits.y);
                args.SetFloatValue(next);
            }

            if (hasUnit)
                FieldUnitHelper.DrawUnit(valueRect, unit);

            const float barH = 9f;
            var barRectY = sliderRect.y + sliderRect.height - 2f;
            var leftRect = new Rect(sliderRect.x, barRectY - 6, 42, barH);
            var rightRect = new Rect(sliderRect.xMax - 42, barRectY - 6, 42, barH);

            var old = args.MiniGray.alignment;
            args.MiniGray.alignment = TextAnchor.UpperLeft;
            GUI.Label(leftRect, args.Property.rangeLimits.x.ToString("0.###"), args.MiniGray);
            args.MiniGray.alignment = TextAnchor.UpperRight;
            GUI.Label(rightRect, args.Property.rangeLimits.y.ToString("0.###"), args.MiniGray);
            args.MiniGray.alignment = old;

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }
    }
}


