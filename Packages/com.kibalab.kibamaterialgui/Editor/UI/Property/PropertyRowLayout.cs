#nullable enable

using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    public readonly struct PropertyRowLayout
    {
        public readonly Rect TotalRect;
        public readonly Rect MainRect;
        public readonly Rect FirstLineRect;
        public readonly Rect LabelRect;
        public readonly Rect FieldRect;
        public readonly Rect ResetRect;

        public bool IsValid => MainRect.width > 0f && MainRect.height > 0f;

        public PropertyRowLayout(
            Rect totalRect,
            Rect mainRect,
            Rect firstLineRect,
            Rect labelRect,
            Rect fieldRect,
            Rect resetRect)
        {
            TotalRect = totalRect;
            MainRect = mainRect;
            FirstLineRect = firstLineRect;
            LabelRect = labelRect;
            FieldRect = fieldRect;
            ResetRect = resetRect;
        }
    }

    internal readonly struct PropertyRowDecorators
    {
        public readonly float TopSpacePx;
        public readonly bool DrawDivider;

        public PropertyRowDecorators(float topSpacePx, bool drawDivider)
        {
            TopSpacePx = topSpacePx;
            DrawDivider = drawDivider;
        }
    }

    internal static class PropertyRowLayoutCalculator
    {
        private const float DepthIndent = 14f;
        private const float ResetWidth = 18f;
        private const float ResetSpacing = 4f;
        private const float LabelGap = 4f;

        public static PropertyRowLayout Build(Rect totalRect, float contentHeight, int depth)
        {
            var leftPad = Mathf.Max(0f, depth) * DepthIndent;
            var rightReserved = ResetWidth + ResetSpacing;

            var mainRect = new Rect(
                totalRect.x + leftPad,
                totalRect.y,
                Mathf.Max(0f, totalRect.width - leftPad - rightReserved),
                Mathf.Max(0f, contentHeight));

            var firstLineHeight = Mathf.Min(mainRect.height, EditorGUIUtility.singleLineHeight + 6f);
            var firstLineRect = new Rect(mainRect.x, mainRect.y, mainRect.width, Mathf.Max(0f, firstLineHeight));

            var labelWidth = Mathf.Clamp(mainRect.width * 0.42f, 120f, 220f);
            labelWidth = Mathf.Min(labelWidth, Mathf.Max(0f, mainRect.width - LabelGap));

            var labelRect = new Rect(
                firstLineRect.x,
                firstLineRect.y,
                Mathf.Max(0f, labelWidth),
                firstLineRect.height);

            var fieldRect = new Rect(
                labelRect.xMax + LabelGap,
                firstLineRect.y,
                Mathf.Max(0f, firstLineRect.width - labelRect.width - LabelGap),
                firstLineRect.height);

            var resetRect = new Rect(
                totalRect.xMax - ResetWidth,
                mainRect.y + Mathf.Max(0f, (mainRect.height - ResetWidth) * 0.5f),
                ResetWidth,
                ResetWidth);

            return new PropertyRowLayout(totalRect, mainRect, firstLineRect, labelRect, fieldRect, resetRect);
        }
    }

    public static class PropertyRowGui
    {
        public static Rect DrawLabelAndGetFieldRect(in PropertyRendererArgs args)
        {
            if (args.Layout.IsValid)
            {
                if (args.Layout.LabelRect.width > 0f)
                    EditorGUI.LabelField(args.Layout.LabelRect, args.Label);
                return args.Layout.FieldRect;
            }

            return EditorGUI.PrefixLabel(args.Position, new GUIContent(args.Label));
        }
    }
}


