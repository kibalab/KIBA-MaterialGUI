using System;
using System.Collections.Generic;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using KIBA_.KIBAMaterialGUI.Editor.Localization;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using KIBA_.KIBAMaterialGUI.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal class GroupTreeView
    {
        private const float DepthIndent = 14f;

        private readonly PresetController _presets;

        private bool _mouseDownOnHeader;

        public GroupTreeView(PresetController presets)
        {
            _presets = presets;
        }

        public void Draw(EditorContext ctx, TreeNode root)
        {
            DrawNode(ctx, root, 0);

            foreach (var prop in root.Properties)
            {
                if (!PassSearch(ctx, prop)) continue;

                ExtensionRegistry.Draw(
                    HookPoint.BeforeProperty,
                    new InjectionArgs(HookPoint.BeforeProperty, ctx, null, prop.Property, prop.Label, 0));

                DrawPropertyRow(ctx, prop.Property,
                    LocalizationHelper.TranslateProp(ctx, prop.Property.name, prop.Label), 0);

                ExtensionRegistry.Draw(
                    HookPoint.AfterProperty,
                    new InjectionArgs(HookPoint.AfterProperty, ctx, null, prop.Property, prop.Label, 0));
            }
        }

        public void DrawGroupHeaderRow(
            EditorContext ctx,
            TreeNode node,
            Rect padded,
            int depth,
            bool enableExportDrag = true,
            bool showRemove = false,
            Action onRemove = null,
            Action onBeginReorderDrag = null)
        {
            GUI.Box(padded, GUIContent.none, ctx.Styles.HeaderButton);

            var groupActions = BuildGroupActions(ctx, node, enableExportDrag);
            var arrowRect = new Rect(padded.x + 6, padded.y + 5, 21, padded.height - 10);
            var rightW = 4f;

            Rect remRect = default;
            if (showRemove)
            {
                rightW += 22f;
                remRect = new Rect(padded.xMax - rightW, padded.y + 4, 20, padded.height - 8);
            }

            rightW += 22f;
            var popRect = new Rect(padded.xMax - rightW, padded.y + 4, 20, padded.height - 8);

            rightW += 22f;
            var optRect = new Rect(padded.xMax - rightW, padded.y + 4, 20, padded.height - 8);

            var actionItems = groupActions.Items
                .Where(static x => x != null && x.Visible)
                .OrderBy(static x => x.Order)
                .ToList();
            var actionRects = new List<(GroupActionItem item, Rect rect)>(actionItems.Count);
            for (var i = 0; i < actionItems.Count; i++)
            {
                rightW += 22f;
                actionRects.Add((actionItems[i], new Rect(padded.xMax - rightW, padded.y + 4, 20, padded.height - 8)));
            }

            var statsWidth = GetGroupStatsWidth(node);
            var statsRect = Rect.zero;
            if (statsWidth > 0f)
            {
                statsRect = new Rect(
                    padded.xMax - rightW - statsWidth,
                    padded.y + 5,
                    statsWidth,
                    padded.height - 10);
                rightW += statsWidth + 6f;
            }

            var labelRect = new Rect(arrowRect.xMax + 4, padded.y + 4, padded.width - (arrowRect.width + rightW + 18f), padded.height - 8);

            var groupOpened = EditorGUIUtility.IconContent("FolderOpened Icon");
            var groupClosed = EditorGUIUtility.IconContent("Folder Icon");
            GUI.Label(arrowRect, node.Expanded ? groupOpened : groupClosed);

            var groupDisplay = LocalizationHelper.TranslateGroup(ctx, node.Name, node.PathKey);
            GUI.Label(labelRect, groupDisplay, EditorStyles.boldLabel);
            DrawGroupStats(statsRect, node);

            var e = Event.current;
            if (e.type == EventType.MouseDown && padded.Contains(e.mousePosition))
                _mouseDownOnHeader = true;

            if (_mouseDownOnHeader && e.type == EventType.MouseDrag && padded.Contains(e.mousePosition))
            {
                _mouseDownOnHeader = false;

                if (enableExportDrag)
                {
                    groupActions.BeginDrag?.Invoke();
                }
                else
                {
                    onBeginReorderDrag?.Invoke();
                }

                e.Use();
            }

            if (e.type == EventType.MouseUp) _mouseDownOnHeader = false;

            var popupIcon = EditorGUIUtility.IconContent("winbtn_win_restore");
            if (GUI.Button(popRect, popupIcon, ctx.Styles.TransparentTextButton))
            {
                GroupPopupWindow.Open(ctx.Material, node.PathKey);
            }

            var optText = EditorGUIUtility.IconContent("_Menu");
            if (GUI.Button(optRect, optText, ctx.Styles.TransparentTextButton))
            {
                ShowGroupMenu(ctx, optRect, node);
            }

            for (var i = 0; i < actionRects.Count; i++)
            {
                var item = actionRects[i].item;
                var rect = actionRects[i].rect;
                using (new EditorGUI.DisabledScope(!item.Enabled))
                {
                    if (item.OnGUI != null)
                    {
                        item.OnGUI(rect);
                    }
                    else if (GUI.Button(rect, item.Content, ctx.Styles.TransparentTextButton))
                    {
                        item.OnClick?.Invoke();
                    }
                }

                if (!string.IsNullOrEmpty(item.Tooltip))
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            }

            if (showRemove)
            {
                var remIcon = EditorGUIUtility.IconContent("winbtn_win_close");
                if (GUI.Button(remRect, remIcon, ctx.Styles.TransparentTextButton))
                {
                    onRemove?.Invoke();
                }
            }

            var foldRect = new Rect(padded.x, padded.y, padded.width - (rightW + 6f), padded.height);
            if (GUI.Button(foldRect, GUIContent.none, GUIStyle.none))
            {
                node.Expanded = !node.Expanded;
                FoldState.SaveFold(ctx.PreferencesKeyPrefix, node.PathKey, node.Expanded);
            }
        }

        private void DrawNode(EditorContext ctx, TreeNode node, int depth)
        {
            foreach (var child in node.Children.Values)
            {
                if (child.Model != null && !child.Model.Visible) continue;

                var headerRect = EditorGUILayout.GetControlRect(false, 28f);
                var padX = depth * DepthIndent;
                var padded = new Rect(headerRect.x + padX, headerRect.y, headerRect.width - padX, headerRect.height);

                ExtensionRegistry.Draw(
                    HookPoint.BeforeGroupHeader,
                    new InjectionArgs(
                        HookPoint.BeforeGroupHeader, ctx, child.PathKey, null, null, depth));

                DrawGroupHeaderRow(ctx, child, padded, depth, true, false, null, null);

                ExtensionRegistry.Draw(
                    HookPoint.AfterGroupHeader,
                    new InjectionArgs(
                        HookPoint.AfterGroupHeader, ctx, child.PathKey, null, null, depth));

                if (!child.Expanded)
                {
                    var line0 = EditorGUILayout.GetControlRect(false, 1f);
                    EditorGUI.DrawRect(new Rect(line0.x + padX, line0.y, line0.width - padX, 1f),
                        new Color(0, 0, 0, EditorGUIUtility.isProSkin ? 0.25f : 0.15f));
                    continue;
                }

                ExtensionRegistry.Draw(
                    HookPoint.BeforeGroupContent,
                    new InjectionArgs(
                        HookPoint.BeforeGroupContent, ctx, child.PathKey, null, null, depth));

                if (child.Children.Count > 0) DrawNode(ctx, child, depth + 1);

                foreach (var prop in child.Properties)
                {
                    if (!PassSearch(ctx, prop)) continue;

                    ExtensionRegistry.Draw(
                        HookPoint.BeforeProperty,
                        new InjectionArgs(
                            HookPoint.BeforeProperty, ctx, child.PathKey, prop.Property, prop.Label, depth + 1));

                    DrawPropertyRow(ctx, prop.Property,
                        LocalizationHelper.TranslateProp(ctx, prop.Property.name, prop.Label), depth + 1);

                    ExtensionRegistry.Draw(
                        HookPoint.AfterProperty,
                        new InjectionArgs(
                            HookPoint.AfterProperty, ctx, child.PathKey, prop.Property, prop.Label, depth + 1));
                }

                ExtensionRegistry.Draw(
                    HookPoint.AfterGroupContent,
                    new InjectionArgs(
                        HookPoint.AfterGroupContent, ctx, child.PathKey, null, null, depth));

                var line = EditorGUILayout.GetControlRect(false, 1f);
                EditorGUI.DrawRect(new Rect(line.x + padX, line.y, line.width - padX, 1f),
                    new Color(0, 0, 0, EditorGUIUtility.isProSkin ? 0.25f : 0.15f));
            }
        }

        private void ShowGroupMenu(EditorContext ctx, Rect anchor, TreeNode child)
        {
            var props = CollectPropsRecursive(child).Select(x => x.Property).ToList();
            var visibleProps = CollectPropsRecursive(child)
                .Where(static x => x.Model == null || x.Model.Visible)
                .Select(static x => x.Property)
                .ToList();

            var model = new ContextMenuModel();

            model.Add(new ContextMenuItem
            {
                Id = "group.copyInternal",
                Type = ContextMenuItemType.Button,
                Label = "Copy Group Values (internal)",
                OnClickOrToggle = () =>
                {
                    var entry = new PresetController().CapturePreset(ctx, ctx.Material.name + "_" + child.Name, props);
                    EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(entry, true);
                },
                Order = 0
            });

            var hasClip = _presets.HasClipboard();
            model.Add(new ContextMenuItem
            {
                Id = "group.paste",
                Type = ContextMenuItemType.Button,
                Label = "Paste Group Values (apply)",
                Enabled = hasClip,
                OnClickOrToggle = () =>
                {
                    if (!_presets.HasClipboard()) return;
                    var clip = _presets.GetClipboard();
                    if (clip == null) return;
                    _presets.ApplyPresetToProps(ctx, clip, props);
                },
                Order = 10
            });

            model.Add(new ContextMenuItem { Id = "sep.0", Type = ContextMenuItemType.Separator, Order = 19 });

            model.Add(new ContextMenuItem
            {
                Id = "group.copyJson",
                Type = ContextMenuItemType.Button,
                Label = "Copy Group as JSON",
                OnClickOrToggle = () =>
                {
                    var entry = new PresetController().CapturePreset(ctx, ctx.Material.name + "_" + child.Name, props);
                    EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(entry, true);
                },
                Order = 20
            });

            model.Add(new ContextMenuItem
            {
                Id = "group.addPreset",
                Type = ContextMenuItemType.Button,
                Label = "Add Preset from Current (This Group)",
                OnClickOrToggle = () => new PresetController().PromptPresetNameAndSave(ctx, ctx.Material.name + "_" + child.Name, props),
                Order = 30
            });

            model.Add(new ContextMenuItem { Id = "sep.1", Type = ContextMenuItemType.Separator, Order = 39 });

            model.Add(new ContextMenuItem
            {
                Id = "group.resetVisible",
                Type = ContextMenuItemType.Button,
                Label = "Reset Visible Properties",
                Enabled = visibleProps.Count > 0,
                OnClickOrToggle = () => PropertyRowHost.ResetPropertiesToShaderDefaults(ctx, visibleProps),
                Order = 40
            });

            ContributionRegistry.ApplyGroupMenu(ctx, model, child.PathKey);

            var menu = new GenericMenu();
            foreach (var it in model.Items.OrderBy(i => i.Order))
            {
                if (!it.Visible) continue;
                if (it.Type == ContextMenuItemType.Separator)
                {
                    menu.AddSeparator("");
                    continue;
                }

                var enabled = it.Enabled && !it.Locked;
                var label = new GUIContent(it.Label ?? "(Unnamed)");

                if (it.Type == ContextMenuItemType.Toggle)
                {
                    var chk = it.CheckedGetter?.Invoke() ?? false;
                    if (enabled) menu.AddItem(label, chk, () => it.OnClickOrToggle?.Invoke());
                    else menu.AddDisabledItem(label, chk);
                }
                else
                {
                    if (enabled) menu.AddItem(label, false, () => it.OnClickOrToggle?.Invoke());
                    else menu.AddDisabledItem(label);
                }
            }

            menu.DropDown(anchor);
        }

        private IEnumerable<TreeNodeProperty> CollectPropsRecursive(TreeNode node)
        {
            foreach (var np in node.Properties) yield return np;
            foreach (var c in node.Children.Values)
            foreach (var x in CollectPropsRecursive(c))
                yield return x;
        }

        private static bool PassSearch(EditorContext ctx, TreeNodeProperty np)
        {
            if (np?.Model != null) return np.Model.Visible;
            var s = ctx.State != null ? ctx.State.Search : ctx.Search;
            if (string.IsNullOrEmpty(s)) return true;
            return np.Property.name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0
                || np.Label.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static GroupActionModel BuildGroupActions(EditorContext ctx, TreeNode node, bool enableExportDrag)
        {
            var model = new GroupActionModel();
            if (enableExportDrag)
                ContributionRegistry.ApplyGroupActions(ctx, model, node.PathKey);
            return model;
        }

        private static float GetGroupStatsWidth(TreeNode node)
        {
            var model = node.Model;
            if (model == null) return 0f;

            var width = 0f;
            if (model.TotalPropertyCount > 0) width += 48f;
            if (model.WarningCount > 0) width += 30f;
            if (model.HasMixed) width += 24f;
            return width;
        }

        private static void DrawGroupStats(Rect rect, TreeNode node)
        {
            var model = node.Model;
            if (model == null || rect.width <= 0f) return;

            var x = rect.x;
            if (model.TotalPropertyCount > 0)
                DrawStatText(ref x, rect, $"{model.VisiblePropertyCount}/{model.TotalPropertyCount}", "Visible properties / total properties", 44f);
            if (model.WarningCount > 0)
                DrawStatText(ref x, rect, $"! {model.WarningCount}", "Properties with warnings", 26f);
            if (model.HasMixed)
                DrawStatText(ref x, rect, "M", "Mixed values in selection", 18f);
        }

        private static void DrawStatText(ref float x, Rect rowRect, string text, string tooltip, float width)
        {
            var rect = new Rect(x, rowRect.y, width, rowRect.height);
            x += width + 4f;

            var oldColor = GUI.contentColor;
            GUI.contentColor = EditorGUIUtility.isProSkin
                ? new Color(0.28f, 0.28f, 0.28f, 1f)
                : new Color(0.16f, 0.16f, 0.16f, 1f);
            GUI.Label(rect, new GUIContent(text, tooltip), EditorStyles.centeredGreyMiniLabel);
            GUI.contentColor = oldColor;
        }

        private void DrawPropertyRow(EditorContext ctx, MaterialProperty property, string label, int depth)
        {
            PropertyRowHost.Draw(ctx, property, label, depth);
        }
    }
}



