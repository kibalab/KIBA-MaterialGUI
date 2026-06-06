using System.Linq;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Samples.Editor
{
    [ShaderEditorInjection(HookPoint.AfterToolbar, Order = 10, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" })]
    public sealed class DemoToolbarInjector : IShaderEditor
    {
        public void OnGUI(InjectionArgs a)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var mat = a.Context.Material;
                bool on = mat.IsKeywordEnabled("_MY_FEATURE");
                if (GUILayout.Button(on ? "Disable _MY_FEATURE" : "Enable _MY_FEATURE", GUILayout.Width(160)))
                {
                    Undo.RecordObject(mat, "Toggle Demo Keyword");
                    if (on) mat.DisableKeyword("_MY_FEATURE");
                    else mat.EnableKeyword("_MY_FEATURE");
                    EditorUtility.SetDirty(mat);
                }

                GUILayout.Label($"Keyword: {(on ? "ON" : "OFF")}", GUILayout.Width(90));
                GUILayout.FlexibleSpace();
            }
        }
    }

    [ShaderEditorInjection(HookPoint.BeforeGroupContent, Order = 0, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" }, GroupPath = "Wind")]
    public sealed class DemoWindGroupInjector : IShaderEditor
    {
        public void OnGUI(InjectionArgs a)
        {
            var pStrength = a.Context.Properties.FirstOrDefault(p => p.name == "_WindStrength");
            var pSpeed = a.Context.Properties.FirstOrDefault(p => p.name == "_WindSpeed");

            EditorGUILayout.HelpBox("Quick wind controls for demo shader.", MessageType.Info);

            if (pStrength != null)
            {
                UnityEditor.EditorGUI.BeginChangeCheck();
                var v = EditorGUILayout.Slider("Wind Boost", pStrength.floatValue, 0f, 2f);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    MaterialGUIPropertyChangeUtility.SetFloat(a.Context, pStrength, v, "Change Wind Strength");
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Randomize", GUILayout.Width(100)))
                {
                    if (pStrength != null)
                        MaterialGUIPropertyChangeUtility.SetFloat(a.Context, pStrength, Random.Range(0f, 2f), "Randomize Wind");
                    if (pSpeed != null)
                        MaterialGUIPropertyChangeUtility.SetFloat(a.Context, pSpeed, Random.Range(0.2f, 8f), "Randomize Wind");
                }

                if (GUILayout.Button("Calm", GUILayout.Width(80)))
                {
                    if (pStrength != null)
                        MaterialGUIPropertyChangeUtility.SetFloat(a.Context, pStrength, 0f, "Calm Wind");
                    if (pSpeed != null)
                        MaterialGUIPropertyChangeUtility.SetFloat(a.Context, pSpeed, 1f, "Calm Wind");
                }

                GUILayout.FlexibleSpace();
            }
        }
    }

    [ShaderEditorInjection(HookPoint.AfterProperty, Order = 0, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" }, GroupPath = "Gradient", PropertyName = "_ColorA")]
    public sealed class DemoGradientPropertyInjector : IShaderEditor
    {
        public void OnGUI(InjectionArgs a)
        {
            var pA = a.Context.Properties.FirstOrDefault(p => p.name == "_ColorA");
            var pB = a.Context.Properties.FirstOrDefault(p => p.name == "_ColorB");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy A → B", GUILayout.Width(100)) && pA != null && pB != null)
                {
                    MaterialGUIPropertyChangeUtility.SetColor(a.Context, pB, pA.colorValue, "Copy Gradient Color");
                }

                if (GUILayout.Button("Swap A ↔ B", GUILayout.Width(100)) && pA != null && pB != null)
                {
                    var colorA = pA.colorValue;
                    MaterialGUIPropertyChangeUtility.SetColor(a.Context, pA, pB.colorValue, "Swap Gradient Colors");
                    MaterialGUIPropertyChangeUtility.SetColor(a.Context, pB, colorA, "Swap Gradient Colors");
                }

                GUILayout.FlexibleSpace();
            }
        }
    }

    [MaterialGUIContribution(ContributionTarget.Toolbar, Order = 5, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" })]
    public sealed class AddResetAllButton : IToolbarContributor
    {
        public void Contribute(ToolbarModel model, InjectionArgs a)
        {
            model.Add(new ToolbarItem
            {
                Id = "btn.resetAll",
                Type = ToolbarItemType.Button,
                Label = "Reset All",
                Width = 90f,
                OnClick = () =>
                {
                    var mat = a.Context.Material;
                    if (mat == null || mat.shader == null) return;
                    var def = new Material(mat.shader);
                    Undo.RecordObject(mat, "Reset All Properties");
                    foreach (var p in a.Context.Properties)
                    {
                        switch (p.type)
                        {
                            case MaterialProperty.PropType.Float:
                            case MaterialProperty.PropType.Range:
                                MaterialGUIPropertyChangeUtility.SetFloat(a.Context, p, def.HasProperty(p.name) ? def.GetFloat(p.name) : 0f, "Reset All Properties"); break;
                            case MaterialProperty.PropType.Color:
                                MaterialGUIPropertyChangeUtility.SetColor(a.Context, p, def.HasProperty(p.name) ? def.GetColor(p.name) : Color.white, "Reset All Properties"); break;
                            case MaterialProperty.PropType.Vector:
                                MaterialGUIPropertyChangeUtility.SetVector(a.Context, p, def.HasProperty(p.name) ? def.GetVector(p.name) : Vector4.zero, "Reset All Properties"); break;
                            case MaterialProperty.PropType.Texture:
                                MaterialGUIPropertyChangeUtility.SetTexture(a.Context, p, def.HasProperty(p.name) ? def.GetTexture(p.name) : null, "Reset All Properties"); break;
                        }
                    }

                    Object.DestroyImmediate(def);
                    EditorUtility.SetDirty(mat);
                },
                Order = 15
            });
        }
    }

    [MaterialGUIContribution(ContributionTarget.Toolbar, Order = 6, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" })]
    public sealed class HidePresetsOnDemo : IToolbarContributor
    {
        public void Contribute(ToolbarModel model, InjectionArgs a)
        {
            model.Hide("presets");
        }
    }

    [MaterialGUIContribution(ContributionTarget.Toolbar, Order = 7, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" })]
    public sealed class LockDoubleSidedOnDemo : IToolbarContributor
    {
        public void Contribute(ToolbarModel model, InjectionArgs a)
        {
            model.Lock("double_sided_toggle", true);
        }
    }

    [MaterialGUIContribution(ContributionTarget.Filter, Order = 20, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" })]
    public sealed class DemoFilterShortcutContributor : IMaterialGUIFilterProvider
    {
        public void Contribute(ToolbarModel model, InjectionArgs a)
        {
            model.Add(new ToolbarItem
            {
                Id = "sample.filter.gradient",
                Type = ToolbarItemType.Button,
                Label = "Gradient",
                Width = 78f,
                Tooltip = "Sample filter toolbar contribution.",
                OnClick = () => model.Find("search")?.TextSetter?.Invoke("Gradient"),
                Order = 20
            });
        }
    }

    [MaterialGUIContribution(ContributionTarget.Diagnostic, Order = 10, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" })]
    public sealed class DemoKeywordDiagnosticContributor : IMaterialGUIDiagnosticProvider
    {
        public void Contribute(List<MaterialGUIDiagnostic> diagnostics, InjectionArgs a)
        {
            var material = a.Context.Material;
            if (material == null || material.IsKeywordEnabled("_MY_FEATURE")) return;

            diagnostics.Add(new MaterialGUIDiagnostic(
                MaterialGUIDiagnosticSeverity.Info,
                "Sample diagnostic: _MY_FEATURE is disabled."));
        }
    }

    [MaterialGUIContribution(ContributionTarget.GroupAction, Order = 10, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" }, GroupPath = "Gradient")]
    public sealed class DemoGradientActionContributor : IMaterialGUIGroupActionContributor
    {
        public void Contribute(GroupActionModel model, InjectionArgs a)
        {
            model.Add(new GroupActionItem
            {
                Id = "gradient.swap",
                Content = new GUIContent("S", "Swap gradient colors"),
                Tooltip = "Swap gradient colors",
                Order = 10,
                OnClick = () =>
                {
                    var pA = a.Context.Properties.FirstOrDefault(p => p.name == "_ColorA");
                    var pB = a.Context.Properties.FirstOrDefault(p => p.name == "_ColorB");
                    if (pA == null || pB == null) return;

                    var colorA = pA.colorValue;
                    MaterialGUIPropertyChangeUtility.SetColor(a.Context, pA, pB.colorValue, "Swap Gradient Colors");
                    MaterialGUIPropertyChangeUtility.SetColor(a.Context, pB, colorA, "Swap Gradient Colors");
                }
            });
        }
    }

    [ShaderEditorInjection(
        HookPoint.AfterGroupHeader, Order = 0,
        ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" }, GroupPath = "Gradient"
    )]
    public sealed class DemoGradientHeaderBadge : IShaderEditor
    {
        public void OnGUI(InjectionArgs a)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(Mathf.Max(0, a.Depth) * 14f + 6f);
                var r = EditorGUILayout.GetControlRect(false, 16f);
                var badge = new GUIContent("Gradient Tools");
                GUI.Label(r, badge, EditorStyles.miniBoldLabel);
            }
        }
    }

    [MaterialGUIContribution(ContributionTarget.GroupContextMenu, Order = 1, ShaderNameEquals = new[] { "KIBA_/Samples/InjectionDemo" }, GroupPath = "Gradient")]
    public sealed class GradientMenuContrib : IGroupMenuContributor
    {
        public void Contribute(ContextMenuModel model, InjectionArgs a)
        {
            model.Add(new ContextMenuItem
            {
                Id = "gradient.pin",
                Type = ContextMenuItemType.Toggle,
                Label = "Pin Gradient Group",
                CheckedGetter = () => EditorPrefs.GetBool("KIBA_.Sample.Pin." + a.Context.Material.shader.name + "." + a.GroupPath, false),
                OnClickOrToggle = () =>
                {
                    var key = "KIBA_.Sample.Pin." + a.Context.Material.shader.name + "." + a.GroupPath;
                    var now = EditorPrefs.GetBool(key, false);
                    EditorPrefs.SetBool(key, !now);
                },
                Order = 5
            });

            model.Add(new ContextMenuItem { Id = "sep.inject", Type = ContextMenuItemType.Separator, Order = 9 });

            model.Add(new ContextMenuItem
            {
                Id = "gradient.quick-contrast",
                Type = ContextMenuItemType.Button,
                Label = "Quick Contrast",
                OnClickOrToggle = () =>
                {
                    var pA = a.Context.Properties.FirstOrDefault(p => p.name == "_ColorA");
                    var pB = a.Context.Properties.FirstOrDefault(p => p.name == "_ColorB");
                    if (pA == null || pB == null) return;
                    var ca = pA.colorValue;
                    var cb = pB.colorValue;
                    cb = new Color(1 - ca.r, 1 - ca.g, 1 - ca.b, cb.a);
                    MaterialGUIPropertyChangeUtility.SetColor(a.Context, pB, cb, "Quick Gradient Contrast");
                },
                Order = 10
            });

            model.Hide("group.copyInternal");
        }
    }
}



