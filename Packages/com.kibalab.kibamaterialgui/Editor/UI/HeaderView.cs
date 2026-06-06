#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal class HeaderView
    {
        public void Draw(EditorContext ctx) => Draw(ctx, true, null);

        public void Draw(EditorContext ctx, bool showAssetIcons, string? customTitle)
        {
            var r = EditorGUILayout.GetControlRect(false, 24f);
            var pad = 6f;

            var langButtonW = 110f;
            var langRect = new Rect(r.xMax - langButtonW - pad, r.y + 3, langButtonW, 18);

            float labelX;
            if (showAssetIcons)
            {
                var matIcon = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Material Icon" : "Material Icon");
                var shIcon = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Shader Icon" : "Shader Icon");

                var iconSize = 18f;
                var matRect = new Rect(r.x + pad, r.y + 3, iconSize, iconSize);
                var shRect = new Rect(matRect.xMax + 4, r.y + 3, iconSize, iconSize);

                GUI.Label(matRect, matIcon);
                GUI.Label(shRect, shIcon);

                if (GUI.Button(matRect, GUIContent.none, GUIStyle.none))
                {
                    if (ctx.Material != null)
                    {
                        Selection.activeObject = ctx.Material;
                        EditorGUIUtility.PingObject(ctx.Material);
                    }
                }

                if (GUI.Button(shRect, GUIContent.none, GUIStyle.none))
                {
                    if (ctx.Material != null && ctx.Material.shader != null)
                    {
                        Selection.activeObject = ctx.Material.shader;
                        EditorGUIUtility.PingObject(ctx.Material.shader);
                    }
                }

                labelX = shRect.xMax + 8f;
            }
            else
            {
                labelX = r.x + pad;
            }

            var labelRect = new Rect(labelX, r.y + 3, Mathf.Max(60, langRect.x - labelX - 6), 18);

            string headerText;
            if (!string.IsNullOrEmpty(customTitle))
            {
                headerText = customTitle!;
            }
            else
            {
                var shaderName = ctx.Material != null && ctx.Material.shader != null ? ctx.Material.shader.name : "(No Shader)";
                headerText = $"Material ({shaderName})";
            }

            GUI.Label(labelRect, headerText, EditorStyles.miniBoldLabel);

            var langLabel = $"{ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:Language", "Language") ?? "Language"}: {ctx.CurrentLanguage}";
            if (GUI.Button(langRect, langLabel, EditorStyles.miniPullDown))
            {
                var menu = new GenericMenu();
                var codes = ctx.LocalizationStore?.Languages is { Count: > 0 }
                    ? ctx.LocalizationStore.Languages.Select(l => l.Code).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    : new List<string> { "EN", "KR", "JP", "ZH" };

                foreach (var code in codes)
                {
                    var c = code;
                    menu.AddItem(new GUIContent(c), string.Equals(ctx.CurrentLanguage, c, StringComparison.OrdinalIgnoreCase), () =>
                    {
                        ctx.CurrentLanguage = c;
                        EditorPrefs.SetString(ctx.PreferencesLanguageKey, ctx.CurrentLanguage);
                        GUI.changed = true;
                        ctx.MaterialEditor?.Repaint();
                    });
                }

                menu.DropDown(langRect);
            }
        }
    }
}


