using System;
using System.Collections.Generic;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class KeywordEnumPropertyRenderer : IMaterialPropertyRenderer
    {
        public bool CanRender(in PropertyRendererArgs args)
        {
            if (args.Property.type != MaterialProperty.PropType.Float) return false;
            var mat = args.Property.targets?.OfType<Material>().FirstOrDefault();
            if (mat == null || mat.shader == null) return false;

            return ShaderPropertyAttributeCache.TryGetKeywordEnum(mat.shader, args.Property.name, out _);
        }

        public float GetHeight(in PropertyRendererArgs args)
            => EditorGUIUtility.singleLineHeight;

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);
            var shader = args.Material != null ? args.Material.shader : args.Shader;

            if (shader == null ||
                !ShaderPropertyAttributeCache.TryGetKeywordEnum(shader, args.Property.name, out var options)
                || options == null || options.Length == 0)
            {
                EditorGUI.BeginChangeCheck();
                int iv = Mathf.Clamp(Mathf.RoundToInt(args.Property.floatValue), 0, 255);
                iv = EditorGUI.IntField(fieldRect, iv);
                if (EditorGUI.EndChangeCheck())
                    SetKeywordEnumIndex(args, iv, Array.Empty<string>());
                return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
            }

            int current = Mathf.Clamp(Mathf.RoundToInt(args.Property.floatValue), 0, options.Length - 1);
            EditorGUI.BeginChangeCheck();
            int next = EditorGUI.Popup(fieldRect, current, options);
            if (EditorGUI.EndChangeCheck())
                SetKeywordEnumIndex(args, next, options);

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }

        private static void SetKeywordEnumIndex(in PropertyRendererArgs args, int newIndex, string[] options)
        {
            var property = args.Property;
            newIndex = Mathf.Max(0, newIndex);
            args.SetFloatValue(newIndex, "Keyword Enum Change");

            var materials = property.targets?.OfType<Material>().ToArray() ?? Array.Empty<Material>();
            if (materials.Length == 0) return;

            string propBase = ToKeywordToken(property.name);

            var nameBased = new List<string>();
            for (int i = 0; i < options.Length; i++)
                nameBased.Add($"{propBase}_{ToKeywordToken(options[i])}");

            var indexBased = new List<string>();
            for (int i = 0; i < Math.Max(options.Length, newIndex + 1); i++)
                indexBased.Add($"{propBase}_{i}");

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
            char[] ch = raw.ToCharArray();
            for (int i = 0; i < ch.Length; i++)
            {
                char c = ch[i];
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_') continue;
                ch[i] = '_';
            }

            return new string(ch);
        }
    }
}


