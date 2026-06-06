#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class VectorPropertyRenderer : IMaterialPropertyRenderer
    {
        private static readonly string[] AxisNames = { "X", "Y", "Z", "W" };
        private const float AxisGap = 6f;
        private const float BadgeWidth = 16f;
        private const float BadgeGap = 2f;
        private const float FieldPadding = 2f;

        private static int s_DragAxisControlId;
        private static float s_DragAxisStartValue;
        private static float s_DragAxisCurrentValue;
        private static Vector2 s_DragAxisStartMouse;

        public bool CanRender(in PropertyRendererArgs args)
        {
            return args.Property.type == MaterialProperty.PropType.Vector;
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var v = args.Property.vectorValue;
            var fieldCount = ResolveFieldCount(args.Material, args.Property);
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);

            EditorGUI.BeginChangeCheck();
            var values = new[] { v.x, v.y, v.z, v.w };
            DrawAxisRow(fieldRect, fieldCount, values);
            if (EditorGUI.EndChangeCheck())
                args.SetVectorValue(new Vector4(values[0], values[1], values[2], values[3]));

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }

        private static void DrawAxisRow(Rect rect, int axisCount, float[] values)
        {
            axisCount = Mathf.Clamp(axisCount, 2, 4);
            var slotW = Mathf.Max(0f, (rect.width - AxisGap * (axisCount - 1)) / axisCount);

            for (var i = 0; i < axisCount; i++)
            {
                var axisRect = new Rect(rect.x + i * (slotW + AxisGap), rect.y, slotW, rect.height);
                values[i] = DrawAxisFloat(axisRect, AxisNames[i], values[i]);
            }
        }

        private static float DrawAxisFloat(Rect rect, string axis, float value)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.textField);

            var inner = new Rect(
                rect.x + FieldPadding,
                rect.y + 1f,
                Mathf.Max(0f, rect.width - FieldPadding * 2f),
                Mathf.Max(0f, rect.height - 2f));

            var badgeRect = new Rect(inner.x, inner.y, BadgeWidth, inner.height);
            var fieldRect = new Rect(
                badgeRect.xMax + BadgeGap,
                inner.y,
                Mathf.Max(0f, inner.width - BadgeWidth - BadgeGap),
                inner.height);

            var controlId = GUIUtility.GetControlID(FocusType.Passive, badgeRect);
            value = HandleAxisLabelDrag(badgeRect, controlId, value);

            GUI.Box(badgeRect, GUIContent.none, EditorStyles.helpBox);
            GUI.Label(badgeRect, axis, EditorStyles.centeredGreyMiniLabel);
            EditorGUIUtility.AddCursorRect(badgeRect, MouseCursor.SlideArrow);

            return EditorGUI.FloatField(fieldRect, GUIContent.none, value);
        }

        private static float HandleAxisLabelDrag(Rect rect, int controlId, float value)
        {
            var e = Event.current;
            if (e == null) return value;

            switch (e.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && rect.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        GUIUtility.keyboardControl = 0;
                        s_DragAxisControlId = controlId;
                        s_DragAxisStartValue = value;
                        s_DragAxisCurrentValue = value;
                        s_DragAxisStartMouse = e.mousePosition;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId && s_DragAxisControlId == controlId)
                    {
                        var delta = e.mousePosition.x - s_DragAxisStartMouse.x;
                        var speed = 0.03f;
                        if (e.shift) speed *= 0.1f;
                        if (e.control || e.command) speed *= 3f;
                        s_DragAxisCurrentValue = s_DragAxisStartValue + delta * speed;
                        GUI.changed = true;
                        e.Use();
                        return s_DragAxisCurrentValue;
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId && s_DragAxisControlId == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        s_DragAxisControlId = 0;
                        EditorGUIUtility.SetWantsMouseJumping(0);
                        e.Use();
                        return s_DragAxisCurrentValue;
                    }
                    break;
            }

            if (GUIUtility.hotControl == controlId && s_DragAxisControlId == controlId)
                return s_DragAxisCurrentValue;

            return value;
        }

        private static int ResolveFieldCount(Material? material, MaterialProperty property)
        {
            if (material != null &&
                ShaderPropertyAttributeCache.TryGetVectorFieldCount(material.shader, property.name, out var n))
            {
                return Mathf.Clamp(n, 2, 4);
            }

            return 4;
        }
    }
}


