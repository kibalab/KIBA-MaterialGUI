#nullable enable

using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal static class FieldUnitHelper
    {
        private const float PadRight = 4f;

        public static bool TryGetUnit(in PropertyRendererArgs args, out string unit)
        {
            unit = string.Empty;
            if (!args.TryGetShaderAttribute("Unit", out var attr)) return false;
            var s = attr.args.Trim();
            if (s.Length >= 2 &&
                ((s[0] == '"' && s[s.Length - 1] == '"') || (s[0] == '\'' && s[s.Length - 1] == '\'')))
                s = s.Substring(1, s.Length - 2).Trim();
            unit = s;
            return !string.IsNullOrEmpty(unit);
        }

        public static void DrawUnit(Rect fieldRect, string unit)
        {
            if (string.IsNullOrEmpty(unit)) return;
            var style = EditorStyles.miniLabel;
            var textWidth = style.CalcSize(new GUIContent(unit)).x;
            var unitRect = new Rect(fieldRect.xMax - textWidth - PadRight, fieldRect.y, textWidth, fieldRect.height);
            var prevColor = GUI.color;
            GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, prevColor.a * 0.5f);
            GUI.Label(unitRect, unit, style);
            GUI.color = prevColor;
        }
    }
}




