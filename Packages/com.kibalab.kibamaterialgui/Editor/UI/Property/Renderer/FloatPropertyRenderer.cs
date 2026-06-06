#nullable enable

using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class FloatPropertyRenderer : IMaterialPropertyRenderer
    {
        public bool CanRender(in PropertyRendererArgs args)
        {
            return args.Property.type == MaterialProperty.PropType.Float;
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var hasUnit = FieldUnitHelper.TryGetUnit(args, out var unit);

            Rect fieldRect;
            if (args.Layout.IsValid)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.FloatField(args.Layout.FirstLineRect, args.Label, args.Property.floatValue);
                if (EditorGUI.EndChangeCheck())
                    args.SetFloatValue(v);

                fieldRect = args.Layout.FieldRect;
            }
            else
            {
                fieldRect = EditorGUI.PrefixLabel(args.Position, new GUIContent(args.Label));
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.FloatField(fieldRect, GUIContent.none, args.Property.floatValue);
                if (EditorGUI.EndChangeCheck())
                    args.SetFloatValue(v);
            }

            if (hasUnit)
                FieldUnitHelper.DrawUnit(fieldRect, unit);

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }
    }
}


