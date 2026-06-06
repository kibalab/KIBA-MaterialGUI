#nullable enable

using System;
using System.Globalization;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class FlexibleRangePropertyRenderer : IMaterialPropertyRenderer
    {
        private const float ValueFieldWidth = 54f;
        private const float ValueFieldGap = 4f;

        public bool CanRender(in PropertyRendererArgs args)
        {
            if (args.Property.type != MaterialProperty.PropType.Float &&
                args.Property.type != MaterialProperty.PropType.Range)
                return false;

            return ShaderPropertyAttributeCache.HasUiHint(
                args.Shader,
                args.Property.name,
                ShaderPropertyAttributeCache.UiHintFlags.FlexibleRange);
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);
            var limits = ResolveLimits(args);
            var hasUnit = FieldUnitHelper.TryGetUnit(args, out var unit);

            var lineRect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            var sliderWidth = Mathf.Max(0f, lineRect.width - ValueFieldWidth - ValueFieldGap);
            var sliderRect = new Rect(lineRect.x, lineRect.y, sliderWidth, lineRect.height);
            var valueRect = new Rect(sliderRect.xMax + ValueFieldGap, lineRect.y, ValueFieldWidth, lineRect.height);

            var current = args.Property.floatValue;
            var outsideRange = IsOutsideRange(current, limits);
            if (outsideRange && !args.Property.hasMixedValue)
            {
                DrawOutOfRangeField(args, lineRect, current, hasUnit ? unit : null);
                return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
            }

            var sliderValue = Mathf.Clamp(current, limits.x, limits.y);

            var prevMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = args.Property.hasMixedValue;

            EditorGUI.BeginChangeCheck();
            var sliderNext = GUI.HorizontalSlider(sliderRect, sliderValue, limits.x, limits.y);
            var sliderChanged = EditorGUI.EndChangeCheck();

            EditorGUI.BeginChangeCheck();
            var fieldNext = EditorGUI.FloatField(valueRect, current);
            var fieldChanged = EditorGUI.EndChangeCheck();

            EditorGUI.showMixedValue = prevMixed;

            if (sliderChanged || fieldChanged)
                args.SetFloatValue(fieldChanged ? fieldNext : sliderNext);

            if (hasUnit)
                FieldUnitHelper.DrawUnit(valueRect, unit);

            DrawLimitLabels(args, sliderRect, limits);
            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }

        private static bool IsOutsideRange(float value, Vector2 limits)
        {
            return value < limits.x && !Mathf.Approximately(value, limits.x) ||
                   value > limits.y && !Mathf.Approximately(value, limits.y);
        }

        private static void DrawOutOfRangeField(
            in PropertyRendererArgs args,
            Rect fieldRect,
            float current,
            string? unit)
        {
            var prevMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = args.Property.hasMixedValue;

            EditorGUI.BeginChangeCheck();
            var next = EditorGUI.FloatField(fieldRect, current);
            var changed = EditorGUI.EndChangeCheck();

            EditorGUI.showMixedValue = prevMixed;

            if (changed)
                args.SetFloatValue(next);

            if (!string.IsNullOrWhiteSpace(unit))
                FieldUnitHelper.DrawUnit(fieldRect, unit);
        }

        private static Vector2 ResolveLimits(in PropertyRendererArgs args)
        {
            if (TryParseAttributeLimits(args, out var limits))
                return limits;

            if (args.Property.type == MaterialProperty.PropType.Range)
                return args.Property.rangeLimits;

            return new Vector2(0f, 1f);
        }

        private static bool TryParseAttributeLimits(in PropertyRendererArgs args, out Vector2 limits)
        {
            limits = default;
            if (!args.TryGetShaderAttribute("FlexibleRange", out var attr) &&
                !args.TryGetShaderAttribute("Flexible", out attr))
                return false;

            var tokens = SplitArgs(attr.args);
            if (tokens.Length < 2)
                return false;

            if (!TryParseFloat(tokens[0], out var min) ||
                !TryParseFloat(tokens[1], out var max))
                return false;

            if (max < min)
            {
                var tmp = min;
                min = max;
                max = tmp;
            }

            if (Mathf.Approximately(min, max))
                max = min + 1f;

            limits = new Vector2(min, max);
            return true;
        }

        private static string[] SplitArgs(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return Array.Empty<string>();

            return args.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool TryParseFloat(string value, out float parsed)
        {
            value = (value ?? string.Empty).Trim().Trim('"', '\'');
            if (TryParseShaderLabNegative(value, out parsed))
                return true;

            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out parsed);
        }

        private static bool TryParseShaderLabNegative(string value, out float parsed)
        {
            parsed = 0f;
            if (string.IsNullOrWhiteSpace(value) ||
                !value.StartsWith("n", StringComparison.OrdinalIgnoreCase) ||
                value.Length <= 1)
                return false;

            var body = value.Substring(1).Replace('_', '.');
            if (!float.TryParse(
                    body,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var positive))
                return false;

            parsed = -positive;
            return true;
        }

        private static void DrawLimitLabels(in PropertyRendererArgs args, Rect sliderRect, Vector2 limits)
        {
            const float barH = 9f;
            var barRectY = sliderRect.y + sliderRect.height - 2f;
            var leftRect = new Rect(sliderRect.x, barRectY - 6, 42, barH);
            var rightRect = new Rect(sliderRect.xMax - 42, barRectY - 6, 42, barH);

            var old = args.MiniGray.alignment;
            args.MiniGray.alignment = TextAnchor.UpperLeft;
            GUI.Label(leftRect, limits.x.ToString("0.###", CultureInfo.InvariantCulture), args.MiniGray);
            args.MiniGray.alignment = TextAnchor.UpperRight;
            GUI.Label(rightRect, limits.y.ToString("0.###", CultureInfo.InvariantCulture), args.MiniGray);
            args.MiniGray.alignment = old;
        }
    }
}
