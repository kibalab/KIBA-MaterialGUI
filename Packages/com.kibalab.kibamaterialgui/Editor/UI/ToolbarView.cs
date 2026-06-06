using System;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal class ToolbarView
    {
        private readonly PresetController _presets;
        private readonly RenderQueueController _rq;
        private readonly List<ToolbarItem> _visibleItemsBuffer = new(16);

        public ToolbarView(PresetController presets, RenderQueueController rq)
        {
            _presets = presets;
            _rq = rq;
        }

        public void Draw(EditorContext ctx, string contributionGroupPath = null, Action<ToolbarModel> configure = null)
        {
            var tGpu = L(ctx, "ui:GPUInstancing", "GPU Instancing");
            var tDs = L(ctx, "ui:DoubleSided", "Double Sided");
            var tExp = L(ctx, "ui:Expand", "Expand");
            var tCol = L(ctx, "ui:Collapse", "Collapse");
            var tPreset = L(ctx, "ui:Presets", "Presets");
            var tRq = L(ctx, "ui:RenderQueue", "Render Queue");

            var targets = GetTargetMaterials(ctx);
            var gpuSupported = targets.Length > 0;
            for (var i = 0; i < targets.Length; i++)
            {
                var mat = targets[i];
                if (mat == null) continue;
                if (ShaderSupportUtil.DetectInstancingSupport(mat.shader)) continue;
                gpuSupported = false;
                break;
            }

            var dsSupported = HasAnyDoubleSidedTarget(targets);

            float viewW = EditorGUIUtility.currentViewWidth;
            float rowH = 20f;
            float gap = 4f;

            float wSearch = Mathf.Clamp(viewW * 0.35f, 160f, 320f);
            string rqLabel() => $"{tRq} {_rq.GetRenderQueueLabel(ctx)}";
            float wRqDrop = EditorStyles.toolbarDropDown.CalcSize(new GUIContent(rqLabel())).x + 12f;
            float wRqField = 64f;

            var model = BuildDefaultModel(
                ctx,
                targets,
                tPreset,
                tExp,
                tCol,
                tGpu,
                tDs,
                wSearch,
                wRqDrop,
                wRqField,
                rqLabel,
                gpuSupported,
                dsSupported);

            ContributionRegistry.ApplyToolbar(ctx, model, contributionGroupPath);
            ContributionRegistry.ApplyFilterToolbar(ctx, model, contributionGroupPath);
            configure?.Invoke(model);

            _visibleItemsBuffer.Clear();
            for (var i = 0; i < model.Items.Count; i++)
            {
                var it = model.Items[i];
                if (!it.Visible) continue;
                _visibleItemsBuffer.Add(it);
            }

            DrawFlow(ctx, _visibleItemsBuffer, rowH, gap);
        }

        private ToolbarModel BuildDefaultModel(
            EditorContext ctx,
            Material[] targets,
            string tPreset, string tExp, string tCol, string tGpu, string tDs,
            float wSearch, float wRqDrop, float wRqField,
            Func<string> rqLabel,
            bool gpuSupported, bool dsSupported)
        {
            var model = new ToolbarModel();
            var hasTargets = targets.Length > 0;
            var presetBaseName = BuildPresetBaseName(ctx, targets);

            model.Add(new ToolbarItem
            {
                Id = "search",
                Type = ToolbarItemType.Input,
                InputKind = InputKind.Text,
                Width = wSearch,
                TextGetter = () => ctx.State != null ? ctx.State.Search : ctx.Search,
                TextSetter = s => SetSearch(ctx, s),
                CustomDrawer = r =>
                {
                    var current = ctx.State != null ? ctx.State.Search : ctx.Search;
                    var next = EditorGUI.TextField(r, current, EditorStyles.toolbarSearchField);
                    if (!string.Equals(current, next, StringComparison.Ordinal))
                        SetSearch(ctx, next);
                },
                Order = 0
            });

            AddFilterDropdown(model, ctx, 1);

            model.Add(new ToolbarItem
            {
                Id = "rq_dropdown",
                Type = ToolbarItemType.Dropdown,
                Width = wRqDrop,
                Enabled = hasTargets,
                DropdownLabelGetter = rqLabel,
                CustomDrawer = r =>
                {
                    using (new EditorGUI.DisabledScope(!hasTargets))
                    {
                        if (GUI.Button(r, rqLabel(), EditorStyles.toolbarDropDown))
                            _rq.ShowRenderQueueMenu(ctx, r);
                    }
                },
                Order = 10
            });

            model.Add(new ToolbarItem
            {
                Id = "rq_field",
                Type = ToolbarItemType.Input,
                InputKind = InputKind.Int,
                Width = wRqField,
                Enabled = hasTargets,
                CustomDrawer = r =>
                {
                    using (new EditorGUI.DisabledScope(!hasTargets))
                    {
                        var currentRq = _rq.GetCurrentRenderQueueValue(ctx);
                        var mixed = currentRq == RenderQueueController.MixedRenderQueue;

                        EditorGUI.BeginChangeCheck();
                        var oldMixed = EditorGUI.showMixedValue;
                        EditorGUI.showMixedValue = mixed;
                        var newRq = EditorGUI.IntField(r, mixed ? 0 : currentRq);
                        EditorGUI.showMixedValue = oldMixed;
                        if (EditorGUI.EndChangeCheck())
                            _rq.SetRenderQueueValue(ctx, newRq);
                    }
                },
                Order = 11
            });

            model.Add(new ToolbarItem
            {
                Id = "gpu_toggle",
                Type = ToolbarItemType.Toggle,
                Label = tGpu,
                Width = 130f,
                Enabled = gpuSupported,
                CustomDrawer = r =>
                {
                    var mixed = !TryGetUniformState(targets, m => m.enableInstancing, out var current);
                    DrawToggleBox(r, tGpu, current, v => SetInstancing(targets, v), gpuSupported, mixed);
                },
                Order = 20
            });

            model.Add(new ToolbarItem
            {
                Id = "double_sided_toggle",
                Type = ToolbarItemType.Toggle,
                Label = tDs,
                Width = 130f,
                Enabled = dsSupported,
                CustomDrawer = r =>
                {
                    var mixed = !TryGetUniformDoubleSidedState(targets, out var current);
                    DrawToggleBox(r, tDs, current, v => SetDoubleSidedForSupported(targets, v), dsSupported, mixed);
                },
                Order = 21
            });

            model.Add(new ToolbarItem
            {
                Id = "expand",
                Type = ToolbarItemType.Button,
                Label = tExp,
                Width = 70f,
                OnClick = () => FoldState.SetAllFolds(ctx.PreferencesKeyPrefix, true),
                Order = 30
            });

            model.Add(new ToolbarItem
            {
                Id = "collapse",
                Type = ToolbarItemType.Button,
                Label = tCol,
                Width = 80f,
                OnClick = () => FoldState.SetAllFolds(ctx.PreferencesKeyPrefix, false),
                Order = 31
            });

            model.Add(new ToolbarItem
            {
                Id = "presets",
                Type = ToolbarItemType.Button,
                Label = tPreset,
                Width = 90f,
                CustomDrawer = r =>
                {
                    if (GUI.Button(r, tPreset, EditorStyles.toolbarButton))
                    {
                        var menu = new GenericMenu();
                        if (ctx.MatchedPresets.Count == 0) menu.AddDisabledItem(new GUIContent(L(ctx, "ui:NoPresets", "No Presets")));
                        else
                        {
                            foreach (var p in ctx.MatchedPresets)
                            {
                                var cap = p;
                                menu.AddItem(new GUIContent($"{L(ctx, "ui:Apply", "Apply")}/{cap.Name}"), false, () => _presets.ApplyPreset(ctx, cap));
                            }
                        }

                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent(L(ctx, "ui:AddPresetFromCurrentAll", "Add Preset from Current (All)")), false,
                            () => _presets.PromptPresetNameAndSave(ctx, presetBaseName + "_Preset", new List<MaterialProperty>(ctx.Properties)));
                        menu.AddItem(new GUIContent(L(ctx, "ui:CopyCurrentAsJsonAll", "Copy Current as JSON (All)")), false,
                            () => _presets.CopyPresetJsonToClipboard(ctx, presetBaseName + "_Preset", new List<MaterialProperty>(ctx.Properties)));
                        menu.DropDown(r);
                    }
                },
                Order = 40
            });

            model.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            return model;
        }

        private static void SetSearch(EditorContext ctx, string search)
        {
            if (ctx.State != null)
            {
                ctx.State.SetSearch(search);
                ctx.Search = ctx.State.Search;
                return;
            }

            ctx.Search = search ?? string.Empty;
        }

        private static void AddFilterDropdown(ToolbarModel model, EditorContext ctx, int order)
        {
            model.Add(new ToolbarItem
            {
                Id = "filter_flags",
                Type = ToolbarItemType.Dropdown,
                Width = 124f,
                CustomDrawer = r =>
                {
                    var label = GetFilterLabel(ctx);
                    if (!GUI.Button(r, label, EditorStyles.toolbarDropDown)) return;

                    var menu = new GenericMenu();
                    var hasFilters = ctx.State != null && ctx.State.Filters != MaterialGUIFilter.None;
                    menu.AddItem(new GUIContent(L(ctx, "ui:FilterAll", "All")), !hasFilters, () => ClearAllFilters(ctx));
                    menu.AddSeparator("");
                    AddFilterOption(menu, ctx, L(ctx, "ui:Changed", "Changed"), MaterialGUIFilter.Changed);
                    AddFilterOption(menu, ctx, L(ctx, "ui:Warnings", "Warnings"), MaterialGUIFilter.Warnings);
                    menu.AddSeparator("");
                    AddFilterOption(menu, ctx, L(ctx, "ui:Textures", "Textures"), MaterialGUIFilter.Textures);
                    AddFilterOption(menu, ctx, L(ctx, "ui:Numbers", "Numbers"), MaterialGUIFilter.Numbers);
                    AddFilterOption(menu, ctx, L(ctx, "ui:Colors", "Colors"), MaterialGUIFilter.Colors);
                    menu.DropDown(r);
                },
                Order = order
            });
        }

        private static void AddFilterOption(GenericMenu menu, EditorContext ctx, string label, MaterialGUIFilter filter)
        {
            var checkedNow = ctx.State != null && ctx.State.HasFilter(filter);
            menu.AddItem(new GUIContent(label), checkedNow, () =>
            {
                if (ctx.State == null) return;
                ctx.State.ToggleFilter(filter);
            });
        }

        private static void ClearAllFilters(EditorContext ctx)
        {
            if (ctx.State == null) return;
            if (ctx.State.Filters == MaterialGUIFilter.None) return;
            ctx.State.Filters = MaterialGUIFilter.None;
            ctx.State.Version++;
        }

        private static string GetFilterLabel(EditorContext ctx)
        {
            var filter = L(ctx, "ui:Filter", "Filter");
            if (ctx.State == null || ctx.State.Filters == MaterialGUIFilter.None)
                return $"{filter}: {L(ctx, "ui:FilterAll", "All")}";

            var count = 0;
            var last = string.Empty;
            if (ctx.State.HasFilter(MaterialGUIFilter.Changed))
            {
                count++;
                last = L(ctx, "ui:Changed", "Changed");
            }
            if (ctx.State.HasFilter(MaterialGUIFilter.Warnings))
            {
                count++;
                last = L(ctx, "ui:Warnings", "Warnings");
            }
            if (ctx.State.HasFilter(MaterialGUIFilter.Textures))
            {
                count++;
                last = L(ctx, "ui:Textures", "Textures");
            }
            if (ctx.State.HasFilter(MaterialGUIFilter.Numbers))
            {
                count++;
                last = L(ctx, "ui:Numbers", "Numbers");
            }
            if (ctx.State.HasFilter(MaterialGUIFilter.Colors))
            {
                count++;
                last = L(ctx, "ui:Colors", "Colors");
            }

            return count == 1 ? $"{filter}: {last}" : $"{filter}: {count}";
        }

        private void DrawToolbarItem(EditorContext ctx, ToolbarItem it, Rect r)
        {
            using (new EditorGUI.DisabledScope(!it.Enabled || it.Locked))
            {
                if (it.CustomDrawer != null)
                {
                    it.CustomDrawer(r);
                    return;
                }

                switch (it.Type)
                {
                    case ToolbarItemType.Input:
                        DrawInput(ctx, r, it);
                        break;
                    case ToolbarItemType.Button:
                        if (GUI.Button(r, it.Label, EditorStyles.toolbarButton)) it.OnClick?.Invoke();
                        break;
                    case ToolbarItemType.Toggle:
                        DrawToggleBox(
                            r,
                            it.Label,
                            it.ToggleGetter?.Invoke() ?? false,
                            v => it.ToggleSetter?.Invoke(v),
                            it.Enabled);
                        break;
                    case ToolbarItemType.Dropdown:
                        if (GUI.Button(r, it.DropdownLabelGetter != null ? it.DropdownLabelGetter() : it.Label, EditorStyles.toolbarDropDown))
                        {
                            var menu = new GenericMenu();
                            if (it.Options != null && it.Options.Count > 0)
                            {
                                foreach (var opt in it.Options)
                                {
                                    if (!opt.Enabled)
                                    {
                                        menu.AddDisabledItem(new GUIContent(opt.Label), opt.CheckedGetter?.Invoke() ?? false);
                                        continue;
                                    }

                                    var checkedNow = opt.CheckedGetter?.Invoke() ?? false;
                                    var o = opt;
                                    menu.AddItem(new GUIContent(o.Label), checkedNow, () => o.OnSelect?.Invoke());
                                }
                            }
                            else
                            {
                                menu.AddDisabledItem(new GUIContent(L(ctx, "ui:Empty", "Empty")));
                            }

                            menu.DropDown(r);
                        }

                        break;
                }
            }
        }

        private void DrawInput(EditorContext ctx, Rect r, ToolbarItem it)
        {
            switch (it.InputKind)
            {
                case InputKind.Text:
                    var t = it.TextGetter != null ? it.TextGetter() : "";
                    var newT = EditorGUI.TextField(r, t);
                    if (newT != t) it.TextSetter?.Invoke(newT);
                    break;
                case InputKind.Int:
                    var i = it.IntGetter != null ? it.IntGetter() : 0;
                    var newI = EditorGUI.IntField(r, i);
                    if (newI != i) it.IntSetter?.Invoke(newI);
                    break;
                case InputKind.Float:
                    var f = it.FloatGetter != null ? it.FloatGetter() : 0f;
                    var newF = EditorGUI.FloatField(r, f);
                    if (!Mathf.Approximately(newF, f)) it.FloatSetter?.Invoke(newF);
                    break;
            }
        }

        private static void DrawToggleBox(Rect r, string label, bool state, Action<bool> set, bool supported, bool mixed = false)
        {
            using (new EditorGUI.DisabledScope(!supported))
            {
                GUI.Box(r, GUIContent.none, EditorStyles.toolbarButton);

                var tRect = new Rect(r.x + 6, r.y, 16, r.height);
                var lRect = new Rect(tRect.xMax + 2, r.y, r.width - 24, r.height);

                var clicked = GUI.Button(r, GUIContent.none, GUIStyle.none);
                var next = state;
                if (clicked && supported) next = mixed ? true : !state;

                var oldMixed = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = mixed;
                GUI.Toggle(tRect, state, GUIContent.none);
                EditorGUI.showMixedValue = oldMixed;

                GUI.Label(lRect, label, EditorStyles.miniLabel);

                if (clicked && next != state || clicked && mixed)
                    set?.Invoke(next);
            }
        }

        private void DrawFlow(EditorContext ctx, List<ToolbarItem> items, float rowHeight, float gap)
        {
            if (items == null || items.Count == 0) return;

            float totalW = EditorGUIUtility.currentViewWidth;
            float innerPad = 6f;
            float maxX = totalW - innerPad;

            int i = 0;
            while (i < items.Count)
            {
                var rowRect = EditorGUILayout.GetControlRect(false, rowHeight);
                GUI.Box(rowRect, GUIContent.none, EditorStyles.toolbar);

                float x = rowRect.x + innerPad;
                float y = rowRect.y + 1f;

                int start = i;
                while (i < items.Count)
                {
                    var item = items[i];
                    float need = Mathf.Max(40f, item.Width);
                    if (x + need > maxX) break;
                    var r = new Rect(x, y, need, rowHeight - 2f);
                    DrawToolbarItem(ctx, item, r);
                    x += need + gap;
                    i++;
                }

                if (start == i && i < items.Count)
                {
                    var item = items[i];
                    float need = Mathf.Min(Mathf.Max(40f, item.Width), maxX - (rowRect.x + innerPad));
                    need = Mathf.Max(need, 60f);
                    var r = new Rect(rowRect.x + innerPad, y, need, rowHeight - 2f);
                    DrawToolbarItem(ctx, item, r);
                    i++;
                }
            }
        }

        private static string BuildPresetBaseName(EditorContext ctx, Material[] targets)
        {
            var baseName = ctx.Material != null ? ctx.Material.name : "Material";
            if (targets.Length <= 1) return baseName;
            return baseName + "_Multi";
        }

        private static bool TryGetUniformState(IReadOnlyList<Material> mats, Func<Material, bool> selector, out bool value)
        {
            value = false;
            if (mats == null || mats.Count == 0) return true;

            value = selector(mats[0]);
            for (int i = 1; i < mats.Count; i++)
            {
                if (selector(mats[i]) != value)
                    return false;
            }

            return true;
        }

        private static bool HasAnyDoubleSidedTarget(IReadOnlyList<Material> targets)
        {
            if (targets == null || targets.Count == 0) return false;
            for (var i = 0; i < targets.Count; i++)
            {
                var mat = targets[i];
                if (mat != null && ShaderSupportUtil.DetectDoubleSidedSupport(mat))
                    return true;
            }

            return false;
        }

        private static bool TryGetUniformDoubleSidedState(IReadOnlyList<Material> targets, out bool value)
        {
            value = false;
            if (targets == null || targets.Count == 0) return true;

            var hasValue = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var mat = targets[i];
                if (mat == null || !ShaderSupportUtil.DetectDoubleSidedSupport(mat)) continue;

                var current = ShaderSupportUtil.GetDoubleSided(mat);
                if (!hasValue)
                {
                    value = current;
                    hasValue = true;
                    continue;
                }

                if (current != value)
                    return false;
            }

            return true;
        }

        private static void SetInstancing(Material[] targets, bool enabled)
        {
            if (targets == null || targets.Length == 0) return;

            var count = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                    count++;
            }

            if (count == 0) return;
            var objects = new Object[count];
            var index = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                var m = targets[i];
                if (m == null) continue;
                objects[index++] = m;
            }

            Undo.RecordObjects(objects, "Toggle GPU Instancing");
            for (int i = 0; i < targets.Length; i++)
            {
                var m = targets[i];
                if (m == null) continue;
                m.enableInstancing = enabled;
                EditorUtility.SetDirty(m);
            }
        }

        private static void SetDoubleSidedForSupported(Material[] targets, bool enabled)
        {
            if (targets == null || targets.Length == 0) return;

            var count = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                var m = targets[i];
                if (m != null && ShaderSupportUtil.DetectDoubleSidedSupport(m))
                    count++;
            }

            if (count == 0) return;
            var objects = new Object[count];
            var index = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                var m = targets[i];
                if (m == null || !ShaderSupportUtil.DetectDoubleSidedSupport(m)) continue;
                objects[index++] = m;
            }

            Undo.RecordObjects(objects, "Toggle Double Sided");
            for (int i = 0; i < targets.Length; i++)
            {
                var m = targets[i];
                if (m == null || !ShaderSupportUtil.DetectDoubleSidedSupport(m)) continue;
                ShaderSupportUtil.SetDoubleSided(m, enabled);
                EditorUtility.SetDirty(m);
            }
        }

        private static Material[] GetTargetMaterials(EditorContext ctx)
        {
            if (ctx?.Targets != null && ctx.Targets.Count > 0)
            {
                var targets = new Material[ctx.Targets.Count];
                for (var i = 0; i < ctx.Targets.Count; i++)
                    targets[i] = ctx.Targets[i];
                return targets;
            }

            if (ctx?.Material != null)
                return new[] { ctx.Material };

            return Array.Empty<Material>();
        }

        private static string L(EditorContext ctx, string key, string fallback)
        {
            return ctx.LocalizationStore?.Get(ctx.CurrentLanguage, key, fallback) ?? fallback;
        }
    }
}


