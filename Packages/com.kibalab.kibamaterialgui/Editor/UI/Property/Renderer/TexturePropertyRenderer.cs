#nullable enable

using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class TexturePropertyRenderer : IMaterialPropertyRenderer
    {
        private const float RowGap = 3f;
        private const float SlotPadding = 2f;
        private const float SlotPreviewSize = 18f;
        private const float SlotHintWidth = 20f;
        private const float ChildBoxPadding = 5f;
        private const float ChildRowGap = 3f;
        private const float ChildLabelWidth = 46f;
        private const float AxisPairGap = 8f;
        private const float AxisBadgeWidth = 16f;
        private const float AxisBadgeGap = 2f;

        private static int s_DragAxisControlId;
        private static float s_DragAxisStartValue;
        private static float s_DragAxisCurrentValue;
        private static Vector2 s_DragAxisStartMouse;

        private enum SlotDragState
        {
            None,
            Valid,
            Invalid
        }

        public bool CanRender(in PropertyRendererArgs args)
        {
            return args.Property.type == MaterialProperty.PropType.Texture &&
                   !GradientTexturePropertyRenderer.IsGradientCandidate(args.Property);
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            var h = GetSlotHeight();
            if (ShouldDrawScaleOffset(args.Property))
                h += RowGap + GetUvGroupHeight();
            return h;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var slotH = GetSlotHeight();
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);
            var slotRect = new Rect(
                fieldRect.x,
                args.Layout.IsValid ? args.Layout.FirstLineRect.y : fieldRect.y,
                fieldRect.width,
                slotH);

            var nextTex = DrawTextureSlot(slotRect, args.Material, args.Property, args.Property.textureValue, args.MiniGray);
            if (nextTex != args.Property.textureValue)
                args.SetTextureValue(nextTex);

            if (ShouldDrawScaleOffset(args.Property))
            {
                var y = (args.Layout.IsValid ? args.Layout.MainRect.y : args.Position.y) + slotH + RowGap;
                var st = args.Property.textureScaleAndOffset;

                var tiling = new Vector2(st.x, st.y);
                var offset = new Vector2(st.z, st.w);

                var groupRect = new Rect(
                    slotRect.x,
                    y,
                    Mathf.Max(0f, slotRect.width),
                    GetUvGroupHeight());

                EditorGUI.BeginChangeCheck();
                DrawUvGroup(groupRect, ref tiling, ref offset);
                if (EditorGUI.EndChangeCheck())
                    args.SetTextureScaleAndOffset(new Vector4(tiling.x, tiling.y, offset.x, offset.y));
            }

            return args.Layout.IsValid ? args.Layout.MainRect : args.Position;
        }

        private static float GetSlotHeight()
        {
            return Mathf.Max(EditorGUIUtility.singleLineHeight + 4f, 22f);
        }

        private static float GetUvGroupHeight()
        {
            var rowH = EditorGUIUtility.singleLineHeight;
            return ChildBoxPadding + rowH + ChildRowGap + rowH + ChildBoxPadding;
        }

        private static Texture? DrawTextureSlot(
            Rect slotRect,
            Material? material,
            MaterialProperty property,
            Texture? current,
            GUIStyle miniGray)
        {
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard, slotRect);
            var pickerId = GetPickerControlId(material, property.name);
            HandleObjectPickerEvent(pickerId, ref current);

            var dragState = HandleDragAndDrop(slotRect, ref current);
            DrawSlotVisual(slotRect, current, dragState, miniGray, controlId);

            HandleSlotInput(slotRect, controlId, pickerId, ref current);

            return current;
        }

        private static void HandleSlotInput(Rect slotRect, int controlId, int pickerId, ref Texture? current)
        {
            var e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown && slotRect.Contains(e.mousePosition))
            {
                GUIUtility.keyboardControl = controlId;
                if (e.button == 0)
                {
                    EditorGUIUtility.ShowObjectPicker<Texture>(current, false, string.Empty, pickerId);
                    e.Use();
                    return;
                }
            }

            if (e.type == EventType.KeyDown &&
                GUIUtility.keyboardControl == controlId &&
                (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace))
            {
                current = null;
                e.Use();
            }
        }

        private static SlotDragState HandleDragAndDrop(Rect rect, ref Texture? current)
        {
            var e = Event.current;
            if (e == null) return SlotDragState.None;
            if (!rect.Contains(e.mousePosition)) return SlotDragState.None;
            if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return SlotDragState.None;

            var tex = FindFirstDraggedTexture();
            if (tex == null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                e.Use();
                return SlotDragState.Invalid;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                current = tex;
            }

            e.Use();
            return SlotDragState.Valid;
        }

        private static Texture? FindFirstDraggedTexture()
        {
            var refs = DragAndDrop.objectReferences;
            if (refs == null) return null;

            for (var i = 0; i < refs.Length; i++)
            {
                if (refs[i] is Texture t) return t;
            }

            return null;
        }

        private static void HandleObjectPickerEvent(int pickerId, ref Texture? current)
        {
            var e = Event.current;
            if (e == null) return;
            if (e.type != EventType.ExecuteCommand) return;
            if (e.commandName != "ObjectSelectorUpdated" && e.commandName != "ObjectSelectorClosed") return;
            if (EditorGUIUtility.GetObjectPickerControlID() != pickerId) return;

            current = EditorGUIUtility.GetObjectPickerObject() as Texture;
            e.Use();
        }

        private static int GetPickerControlId(Material? material, string propertyName)
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + (material != null ? material.GetInstanceID() : 0);
                hash = hash * 31 + (propertyName != null ? propertyName.GetHashCode() : 0);
                return hash;
            }
        }

        private static void DrawSlotVisual(Rect rect, Texture? tex, SlotDragState dragState, GUIStyle miniGray, int controlId)
        {
            var e = Event.current;
            var hovered = e != null && rect.Contains(e.mousePosition);
            var focused = GUIUtility.keyboardControl == controlId;

            GUI.Box(rect, GUIContent.none, EditorStyles.textField);

            var inner = new Rect(
                rect.x + SlotPadding,
                rect.y + SlotPadding,
                Mathf.Max(0f, rect.width - SlotPadding * 2f),
                Mathf.Max(0f, rect.height - SlotPadding * 2f));

            var previewSize = Mathf.Min(SlotPreviewSize, inner.height);
            var previewRect = new Rect(
                inner.x,
                inner.y + (inner.height - previewSize) * 0.5f,
                previewSize,
                previewSize);

            GUI.Box(previewRect, GUIContent.none, EditorStyles.helpBox);
            if (tex != null)
            {
                EditorGUI.DrawPreviewTexture(previewRect, tex, null, ScaleMode.ScaleToFit);
            }
            else
            {
                var texIcon = EditorGUIUtility.IconContent("Texture2D Icon");
                GUI.Label(previewRect, texIcon, EditorStyles.centeredGreyMiniLabel);
            }

            var hintRect = new Rect(inner.xMax - SlotHintWidth, inner.y, SlotHintWidth, inner.height);
            var textRect = new Rect(
                previewRect.xMax + 4f,
                inner.y,
                Mathf.Max(0f, hintRect.x - (previewRect.xMax + 4f) - 2f),
                inner.height);

            if (tex != null)
            {
                GUI.Label(textRect, $"{tex.name} ({tex.width}x{tex.height})", EditorStyles.label);
            }
            else
            {
                var text = dragState switch
                {
                    SlotDragState.Valid => "Release to assign",
                    SlotDragState.Invalid => "Texture only",
                    _ => "Click or drop texture"
                };

                var old = miniGray.alignment;
                miniGray.alignment = TextAnchor.MiddleLeft;
                GUI.Label(textRect, text, miniGray);
                miniGray.alignment = old;
            }

            var hint = dragState switch
            {
                SlotDragState.Valid => "v",
                SlotDragState.Invalid => "!",
                _ when hovered || focused => "v",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(hint))
                GUI.Label(hintRect, hint, EditorStyles.centeredGreyMiniLabel);
        }

        private static bool ShouldDrawScaleOffset(MaterialProperty property)
        {
            return (property.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0;
        }

        private static void DrawUvGroup(Rect groupRect, ref Vector2 tiling, ref Vector2 offset)
        {
            GUI.Box(groupRect, GUIContent.none, EditorStyles.helpBox);

            var rowH = EditorGUIUtility.singleLineHeight;
            var inner = new Rect(
                groupRect.x + ChildBoxPadding,
                groupRect.y + ChildBoxPadding,
                Mathf.Max(0f, groupRect.width - ChildBoxPadding * 2f),
                Mathf.Max(0f, groupRect.height - ChildBoxPadding * 2f));

            var contentX = inner.x;
            var contentW = Mathf.Max(0f, inner.xMax - contentX);
            var tilingRow = new Rect(contentX, inner.y, contentW, rowH);
            var offsetRow = new Rect(contentX, tilingRow.yMax + ChildRowGap, contentW, rowH);

            var tilingLabel = new Rect(tilingRow.x, tilingRow.y, ChildLabelWidth, tilingRow.height);
            var tilingField = new Rect(tilingLabel.xMax, tilingRow.y, Mathf.Max(0f, tilingRow.width - ChildLabelWidth), tilingRow.height);
            EditorGUI.LabelField(tilingLabel, "Tiling", EditorStyles.miniLabel);
            tiling = DrawVector2Row(tilingField, tiling, "U", "V");

            var offsetLabel = new Rect(offsetRow.x, offsetRow.y, ChildLabelWidth, offsetRow.height);
            var offsetField = new Rect(offsetLabel.xMax, offsetRow.y, Mathf.Max(0f, offsetRow.width - ChildLabelWidth), offsetRow.height);
            EditorGUI.LabelField(offsetLabel, "Offset", EditorStyles.miniLabel);
            offset = DrawVector2Row(offsetField, offset, "U", "V");
        }

        private static Vector2 DrawVector2Row(Rect rect, Vector2 value, string axis0, string axis1)
        {
            var halfW = Mathf.Max(0f, (rect.width - AxisPairGap) * 0.5f);
            var leftRect = new Rect(rect.x, rect.y, halfW, rect.height);
            var rightRect = new Rect(leftRect.xMax + AxisPairGap, rect.y, halfW, rect.height);

            value.x = DrawAxisFloat(leftRect, axis0, value.x);
            value.y = DrawAxisFloat(rightRect, axis1, value.y);

            return value;
        }

        private static float DrawAxisFloat(Rect rect, string axis, float value)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.textField);

            var inner = new Rect(
                rect.x + SlotPadding,
                rect.y + 1f,
                Mathf.Max(0f, rect.width - SlotPadding * 2f),
                Mathf.Max(0f, rect.height - 2f));

            var badgeRect = new Rect(inner.x, inner.y, AxisBadgeWidth, inner.height);
            var fieldRect = new Rect(
                badgeRect.xMax + AxisBadgeGap,
                inner.y,
                Mathf.Max(0f, inner.width - AxisBadgeWidth - AxisBadgeGap),
                inner.height);

            var controlId = GUIUtility.GetControlID(FocusType.Passive, badgeRect);
            value = HandleAxisLabelDrag(badgeRect, controlId, value);

            GUI.Box(badgeRect, GUIContent.none, EditorStyles.helpBox);
            GUI.Label(badgeRect, axis, EditorStyles.centeredGreyMiniLabel);
            EditorGUIUtility.AddCursorRect(badgeRect, MouseCursor.SlideArrow);

            var next = EditorGUI.FloatField(fieldRect, GUIContent.none, value);
            return next;
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
    }
}


