#nullable enable

using System;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class SegmentedEnumPropertyRenderer : IMaterialPropertyRenderer
    {
        public bool CanRender(in PropertyRendererArgs args)
        {
            if (args.Property.type != MaterialProperty.PropType.Float) return false;
            return ShaderPropertyAttributeCache.HasUiHint(
                args.Shader, args.Property.name, ShaderPropertyAttributeCache.UiHintFlags.SegmentedEnum);
        }

        public float GetHeight(in PropertyRendererArgs args) => EditorGUIUtility.singleLineHeight;

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);

            if (ShaderPropertyAttributeCache.TryGetKeywordEnum(args.Shader, args.Property.name, out var kwOptions)
                && kwOptions.Length > 0)
            {
                DrawKeywordSegmented(fieldRect, args, kwOptions);
            }
            else if (ShaderPropertyAttributeCache.TryGetEnum(args.Shader, args.Property.name, out var enumInfo))
            {
                DrawEnumSegmented(fieldRect, args, enumInfo);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.IntField(fieldRect, GUIContent.none, Mathf.RoundToInt(args.Property.floatValue));
                if (EditorGUI.EndChangeCheck()) args.SetFloatValue(v);
            }

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }

        private static void DrawKeywordSegmented(Rect fieldRect, in PropertyRendererArgs args, string[] options)
        {
            var mixed = args.Property.hasMixedValue;
            var current = mixed ? -1 : Mathf.Clamp(Mathf.RoundToInt(args.Property.floatValue), 0, options.Length - 1);

            var selected = DrawSegments(fieldRect, options, current);
            if (selected >= 0 && selected != current)
                SetKeywordEnumIndex(args, selected, options);
        }

        private static void DrawEnumSegmented(Rect fieldRect, in PropertyRendererArgs args, ShaderPropertyAttributeCache.EnumInfo info)
        {
            var mixed = args.Property.hasMixedValue;
            var curIdx = -1;
            if (!mixed)
            {
                var cur = args.Property.floatValue;
                var best = float.PositiveInfinity;
                for (var i = 0; i < info.values.Length; i++)
                {
                    var d = Mathf.Abs(info.values[i] - cur);
                    if (d < best) { best = d; curIdx = i; }
                }
            }

            var selected = DrawSegments(fieldRect, info.names, curIdx);
            if (selected >= 0 && selected != curIdx)
                args.SetFloatValue(info.values[Mathf.Clamp(selected, 0, info.values.Length - 1)], "Enum Change");
        }

        private static int DrawSegments(Rect fieldRect, string[] labels, int selectedIndex)
        {
            if (labels == null || labels.Length == 0) return -1;

            var count = labels.Length;
            var clicked = -1;

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            for (var i = 0; i < count; i++)
            {
                var x0 = Mathf.Round(fieldRect.x + (float)i / count * fieldRect.width);
                var x1 = Mathf.Round(fieldRect.x + (float)(i + 1) / count * fieldRect.width);
                var segRect = new Rect(x0, fieldRect.y, x1 - x0, fieldRect.height);

                GUIStyle style;
                if (count == 1)            style = EditorStyles.miniButton;
                else if (i == 0)           style = EditorStyles.miniButtonLeft;
                else if (i == count - 1)   style = EditorStyles.miniButtonRight;
                else                       style = EditorStyles.miniButtonMid;

                var isOn = i == selectedIndex;
                if (GUI.Toggle(segRect, isOn, labels[i], style) && !isOn)
                    clicked = i;
            }

            EditorGUI.indentLevel = prevIndent;
            return clicked;
        }

        private static void SetKeywordEnumIndex(in PropertyRendererArgs args, int newIndex, string[] options)
        {
            var property = args.Property;
            newIndex = Mathf.Max(0, newIndex);
            args.SetFloatValue(newIndex, "Keyword Enum Change");

            var materials = property.targets?.OfType<Material>().ToArray() ?? Array.Empty<Material>();
            if (materials.Length == 0) return;

            var propBase = ToKeywordToken(property.name);

            var nameBased = new string[options.Length];
            for (var i = 0; i < options.Length; i++)
                nameBased[i] = $"{propBase}_{ToKeywordToken(options[i])}";

            var indexCount = Math.Max(options.Length, newIndex + 1);
            var indexBased = new string[indexCount];
            for (var i = 0; i < indexCount; i++)
                indexBased[i] = $"{propBase}_{i}";

            foreach (var mat in materials)
            {
                foreach (var kw in nameBased) mat.DisableKeyword(kw);
                foreach (var kw in indexBased) mat.DisableKeyword(kw);

                if (options.Length > 0 && newIndex < options.Length)
                    mat.EnableKeyword($"{propBase}_{ToKeywordToken(options[newIndex])}");
                mat.EnableKeyword($"{propBase}_{newIndex}");
                EditorUtility.SetDirty(mat);
            }
        }

        private static string ToKeywordToken(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            raw = raw.ToUpperInvariant();
            var ch = raw.ToCharArray();
            for (var i = 0; i < ch.Length; i++)
            {
                var c = ch[i];
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_') continue;
                ch[i] = '_';
            }
            return new string(ch);
        }
    }
}


