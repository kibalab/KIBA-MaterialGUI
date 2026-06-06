#nullable enable

using System.Globalization;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class MinMaxSliderPropertyRenderer : IMaterialPropertyRenderer
    {
        private const float NumFieldW = 50f;
        private const float Gap = 4f;

        public bool CanRender(in PropertyRendererArgs args)
        {
            if (args.Property.type != MaterialProperty.PropType.Vector) return false;
            return ShaderPropertyAttributeCache.HasUiHint(
                args.Shader, args.Property.name, ShaderPropertyAttributeCache.UiHintFlags.MinMaxSlider);
        }

        public float GetHeight(in PropertyRendererArgs args) => EditorGUIUtility.singleLineHeight;

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);

            ParseRange(args, out var rangeMin, out var rangeMax);

            var vec = args.Property.vectorValue;
            var minVal = Mathf.Clamp(vec.x, rangeMin, rangeMax);
            var maxVal = Mathf.Clamp(vec.y, rangeMin, rangeMax);

            var lineRect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            SplitRects(lineRect, out var minFieldRect, out var sliderRect, out var maxFieldRect);

            EditorGUI.BeginChangeCheck();

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            minVal = EditorGUI.FloatField(minFieldRect, minVal);
            maxVal = EditorGUI.FloatField(maxFieldRect, maxVal);
            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, rangeMin, rangeMax);

            EditorGUI.indentLevel = prevIndent;

            minVal = Mathf.Clamp(minVal, rangeMin, Mathf.Min(maxVal, rangeMax));
            maxVal = Mathf.Clamp(maxVal, Mathf.Max(minVal, rangeMin), rangeMax);

            if (EditorGUI.EndChangeCheck())
                args.SetVectorValue(new Vector4(minVal, maxVal, vec.z, vec.w));

            var barRectY = lineRect.y + EditorGUIUtility.singleLineHeight - 2f;
            const float barH = 9f;
            var leftLabelRect  = new Rect(sliderRect.x,          barRectY - 6f, 42f, barH);
            var rightLabelRect = new Rect(sliderRect.xMax - 42f, barRectY - 6f, 42f, barH);

            var old = args.MiniGray.alignment;
            args.MiniGray.alignment = TextAnchor.UpperLeft;
            GUI.Label(leftLabelRect,  rangeMin.ToString("0.###", CultureInfo.InvariantCulture), args.MiniGray);
            args.MiniGray.alignment = TextAnchor.UpperRight;
            GUI.Label(rightLabelRect, rangeMax.ToString("0.###", CultureInfo.InvariantCulture), args.MiniGray);
            args.MiniGray.alignment = old;

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }

        private static void SplitRects(Rect line, out Rect minField, out Rect slider, out Rect maxField)
        {
            var sliderW = Mathf.Max(0f, line.width - NumFieldW * 2f - Gap * 2f);
            minField = new Rect(line.x,                               line.y, NumFieldW, line.height);
            slider   = new Rect(line.x + NumFieldW + Gap,             line.y, sliderW,   line.height);
            maxField = new Rect(line.x + NumFieldW + Gap + sliderW + Gap, line.y, NumFieldW, line.height);
        }

        private static void ParseRange(in PropertyRendererArgs args, out float rangeMin, out float rangeMax)
        {
            rangeMin = 0f;
            rangeMax = 1f;

            if (!args.TryGetShaderAttribute("MinMaxSlider", out var attr)) return;
            var raw = attr.args.Trim();
            if (string.IsNullOrEmpty(raw)) return;

            var comma = raw.IndexOf(',');
            if (comma < 0)
            {
                rangeMax = ParseToken(raw);
                return;
            }

            rangeMin = ParseToken(raw.Substring(0, comma).Trim());
            rangeMax = ParseToken(raw.Substring(comma + 1).Trim());

            if (rangeMin > rangeMax) (rangeMin, rangeMax) = (rangeMax, rangeMin);
        }

        private static float ParseToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return 0f;
            var negative = token.Length > 1 && token[0] == 'n' && char.IsDigit(token[1]);
            var numStr = negative ? token.Substring(1) : token;
            float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var val);
            return negative ? -val : val;
        }
    }
}


