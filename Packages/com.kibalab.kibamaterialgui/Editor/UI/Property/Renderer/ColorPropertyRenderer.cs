#nullable enable

using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class ColorPropertyRenderer : IMaterialPropertyRenderer
    {
        public bool CanRender(in PropertyRendererArgs args)
        {
            return args.Property.type == MaterialProperty.PropType.Color;
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);
            var prevMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = args.Property.hasMixedValue;
            var hdr = (args.Property.flags & MaterialProperty.PropFlags.HDR) != 0;

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var c = EditorGUI.ColorField(fieldRect, GUIContent.none, args.Property.colorValue, true, true, hdr);
            EditorGUI.indentLevel = prevIndent;

            EditorGUI.showMixedValue = prevMixed;

            if (args.Property.colorValue != c)
                args.SetColorValue(c);

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }
    }
}


