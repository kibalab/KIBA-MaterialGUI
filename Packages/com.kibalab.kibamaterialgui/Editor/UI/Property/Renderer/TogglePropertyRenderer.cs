#nullable enable
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class TogglePropertyRenderer : IMaterialPropertyRenderer
    {
        public bool CanRender(in PropertyRendererArgs args)
        {
            if (args.Property.type != MaterialProperty.PropType.Float) return false;

            var mat = args.Property.targets != null && args.Property.targets.Length > 0 ? args.Property.targets[0] as Material : null;
            var shader = mat != null ? mat.shader : null;
            if (ShaderPropertyAttributeCache.TryGetEnum(shader, args.Property.name, out _)) return false;

            return ShaderPropertyAttributeCache.TryGetToggle(shader, args.Property.name, out _);
        }

        public float GetHeight(in PropertyRendererArgs args)
            => EditorGUIUtility.singleLineHeight;

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            var fieldRect = PropertyRowGui.DrawLabelAndGetFieldRect(args);
            var mats = CollectMaterials(args.Property);
            var shader = args.Material != null ? args.Material.shader : (mats.Count > 0 ? mats[0].shader : null);
            ShaderPropertyAttributeCache.TryGetToggle(shader, args.Property.name, out var tinfo);

            var current = args.Property.floatValue > 0.5f;
            var mixed = IsMixedToggleState(mats, args.Property.name, current);

            EditorGUI.BeginChangeCheck();
            var oldMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixed;
            var next = EditorGUI.Toggle(fieldRect, current);
            EditorGUI.showMixedValue = oldMixed;
            if (EditorGUI.EndChangeCheck())
            {
                args.SetFloatValue(next ? 1f : 0f, "Toggle Change");

                var baseKw = !string.IsNullOrEmpty(tinfo.keyword) ? tinfo.keyword : ToKeywordToken(args.Property.name);
                bool kwOn = tinfo.invert ? !next : next;

                for (int i = 0; i < mats.Count; i++)
                {
                    var m = mats[i];
                    if (!m) continue;

                    if (kwOn)
                    {
                        m.EnableKeyword(baseKw);
                        m.EnableKeyword(baseKw + "_ON");
                        m.DisableKeyword(baseKw + "_OFF");
                    }
                    else
                    {
                        m.DisableKeyword(baseKw);
                        m.DisableKeyword(baseKw + "_ON");
                        m.EnableKeyword(baseKw + "_OFF");
                    }

                    EditorUtility.SetDirty(m);
                }
            }

            return args.Layout.IsValid ? args.Layout.FirstLineRect : args.Position;
        }

        private static List<Material> CollectMaterials(MaterialProperty prop)
        {
            var list = new List<Material>(prop.targets != null ? prop.targets.Length : 0);
            var targets = prop.targets;
            if (targets != null)
            {
                for (int i = 0; i < targets.Length; i++)
                    if (targets[i] is Material m)
                        list.Add(m);
            }

            return list;
        }

        private static string ToKeywordToken(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            raw = raw.ToUpperInvariant();
            var ch = raw.ToCharArray();
            for (int i = 0; i < ch.Length; i++)
            {
                char c = ch[i];
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_') continue;
                ch[i] = '_';
            }

            return new string(ch);
        }

        private static bool IsMixedToggleState(IReadOnlyList<Material> mats, string propertyName, bool fallback)
        {
            if (mats == null || mats.Count == 0) return false;

            var first = fallback;
            var firstFound = false;
            for (var i = 0; i < mats.Count; i++)
            {
                var m = mats[i];
                if (!m || !m.HasProperty(propertyName)) continue;
                var v = m.GetFloat(propertyName) > 0.5f;
                if (!firstFound)
                {
                    first = v;
                    firstFound = true;
                    continue;
                }

                if (v != first) return true;
            }

            return false;
        }
    }
}


