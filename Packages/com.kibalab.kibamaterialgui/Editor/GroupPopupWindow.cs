using System.IO;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using KIBA_.KIBAMaterialGUI.Editor.IO;
using KIBA_.KIBAMaterialGUI.Editor.Localization;
using KIBA_.KIBAMaterialGUI.Editor.Parsing;
using KIBA_.KIBAMaterialGUI.Editor.UI;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Windows
{
    internal sealed class GroupPopupWindow : EditorWindow
    {
        private const string PreferencesKeyPrefix = "KIBA_.KIBAMaterialGUI.";
        private const string PreferencesLanguageKey = PreferencesKeyPrefix + "Lang";

        [SerializeField] private string _materialGuid;
        [SerializeField] private string _groupPath;
        [SerializeField] private bool _showAllGroups;

        private Material _material;

        private MaterialEditor _matEditor;
        private IMaterialPropertyDisplayParser _displayParser;
        private ShaderPresetFileService _presetFileService;
        private ShaderLocalizationFileService _localizationFileService;
        private MaterialPropertyRendererRegistry _rendererRegistry;

        private ShaderPresetStore _presetStore;
        private ShaderLocalizationStore _localizationStore;
        private string _loadedStoreRootPath;

        private MaterialGUIStyles _styles;
        private ToolbarView _toolbar;
        private GroupTreeView _tree;
        private HeaderView _header;
        private PresetController _presets;
        private RenderQueueController _renderQueue;

        private string RootPath => _material != null && _material.shader != null
            ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(_material.shader))
            : "Assets";

        private string PrefKey_ShowAll() => $"{PreferencesKeyPrefix}GroupWin.ShowAll.{_materialGuid}";

        public static void Open(Material mat, string groupPath)
        {
            var w = CreateWindow<GroupPopupWindow>();
            w._material = mat;
            w._groupPath = groupPath;
            w._materialGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mat));
            w._showAllGroups = EditorPrefs.GetBool(w.PrefKey_ShowAll(), false);
            w.minSize = new Vector2(480, 260);
            w.ReloadStoresIfNeeded();
            w.SetTitle();
            w.Show();
            w.Focus();
        }

        private void ResolveMaterialIfNeeded()
        {
            if (_material != null) return;
            if (string.IsNullOrEmpty(_materialGuid)) return;
            var path = AssetDatabase.GUIDToAssetPath(_materialGuid);
            _material = AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        private void SetTitle()
        {
            var icon = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Material On Icon" : "Material Icon")?.image as Texture2D;
            var matName = _material != null ? _material.name : "(Material)";
            var groupName = _groupPath;
            if (_localizationStore != null)
            {
                var language = EditorPrefs.GetString(PreferencesLanguageKey, _localizationStore.DefaultCode ?? "EN");
                groupName = _localizationStore.Get(language, $"group:{_groupPath}", _groupPath);
            }

            titleContent = new GUIContent($"{matName}/{groupName}", icon);
        }

        private void OnEnable()
        {
            ResolveMaterialIfNeeded();

            if (_material != null)
                _matEditor = (MaterialEditor)UnityEditor.Editor.CreateEditor(_material, typeof(MaterialEditor));

            _displayParser ??= new MaterialPropertyDisplayParser();
            _presetFileService ??= new ShaderPresetFileService();
            _localizationFileService ??= new ShaderLocalizationFileService();
            _rendererRegistry ??= new MaterialPropertyRendererRegistry();

            _styles ??= new MaterialGUIStyles();
            _styles.Ensure();

            _presets ??= new PresetController();
            _renderQueue ??= new RenderQueueController();
            _tree ??= new GroupTreeView(_presets);
            _toolbar ??= new ToolbarView(_presets, _renderQueue);
            _header ??= new HeaderView();

            if (!string.IsNullOrEmpty(_materialGuid))
                _showAllGroups = EditorPrefs.GetBool(PrefKey_ShowAll(), _showAllGroups);

            ReloadStoresIfNeeded();
            SetTitle();
        }

        private void OnDisable()
        {
            if (_matEditor != null)
            {
                DestroyImmediate(_matEditor);
                _matEditor = null;
            }
        }

        private void OnGUI()
        {
            using (new EditorGUIWideModeScope(true))
            {
                ResolveMaterialIfNeeded();
                EnsureMaterialEditor();
                ReloadStoresIfNeeded();
                SetTitle();

                if (_material == null || _material.shader == null)
                {
                    EditorGUILayout.HelpBox("No Material.", MessageType.Info);
                    return;
                }

                var props = MaterialEditor.GetMaterialProperties(new Object[] { _material });

                var ctx = new EditorContext
                {
                    MaterialEditor = _matEditor,
                    Material = _material,
                    Targets = _material != null ? new[] { _material } : System.Array.Empty<Material>(),
                    Properties = props,
                    DisplayParser = _displayParser,
                    PresetFileService = _presetFileService,
                    LocalizationFileService = _localizationFileService,
                    RendererRegistry = _rendererRegistry,
                    PresetStore = _presetStore,
                    LocalizationStore = _localizationStore,
                    CurrentLanguage = EditorPrefs.GetString(PreferencesLanguageKey, _localizationStore?.DefaultCode ?? "EN"),
                    Styles = _styles,
                    PreferencesKeyPrefix = PreferencesKeyPrefix,
                    PreferencesLanguageKey = PreferencesLanguageKey
                };

                _presets.BuildMatchedPresets(ctx);

                _header.Draw(ctx);

                var tAllGroups = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:ShowAllGroups", "All Groups") ?? "All Groups";
                _toolbar.Draw(ctx, _groupPath, model =>
                {
                    model.Add(new ToolbarItem
                    {
                        Id = "window.showAllGroups",
                        Type = ToolbarItemType.Toggle,
                        Label = tAllGroups,
                        Width = 130f,
                        ToggleGetter = () => _showAllGroups,
                        ToggleSetter = v =>
                        {
                            _showAllGroups = v;
                            if (!string.IsNullOrEmpty(_materialGuid)) EditorPrefs.SetBool(PrefKey_ShowAll(), _showAllGroups);
                            Repaint();
                        },
                        Order = 15
                    });
                });

                EditorGUILayout.Space(6);

                var root = TreeBuilder.Build(ctx);

                if (_showAllGroups)
                {
                    _tree.Draw(ctx, root);
                    return;
                }

                var node = TreeBuilder.FindByPath(root, _groupPath);
                if (node == null)
                {
                    EditorGUILayout.HelpBox($"Group not found: {_groupPath}", MessageType.Warning);
                    return;
                }

                DrawSingleGroup(ctx, node, 0);
            }
        }

        private void EnsureMaterialEditor()
        {
            if (_material == null)
                return;

            if (_matEditor != null && _matEditor.target == _material)
                return;

            if (_matEditor != null)
                DestroyImmediate(_matEditor);

            _matEditor = (MaterialEditor)UnityEditor.Editor.CreateEditor(_material, typeof(MaterialEditor));
        }

        private void ReloadStoresIfNeeded()
        {
            var rootPath = RootPath;
            if (_presetStore != null &&
                _localizationStore != null &&
                string.Equals(_loadedStoreRootPath, rootPath, System.StringComparison.Ordinal))
                return;

            _presetStore = ShaderPresetFileService.Load(rootPath);
            _localizationStore = ShaderLocalizationFileService.Load(rootPath);
            _loadedStoreRootPath = rootPath;
        }

        private void DrawSingleGroup(EditorContext ctx, TreeNode child, int depth)
        {
            ExtensionRegistry.Draw(
                HookPoint.BeforeGroupHeader,
                new InjectionArgs(
                    HookPoint.BeforeGroupHeader, ctx, child.PathKey, null, null, depth));

            var headerRect = EditorGUILayout.GetControlRect(false, 28f);
            var padded = new Rect(headerRect.x, headerRect.y, headerRect.width, headerRect.height);

            GUI.Box(padded, GUIContent.none, ctx.Styles.HeaderButton);

            var arrowRect = new Rect(padded.x + 6, padded.y + 5, 21, padded.height - 10);
            var labelRect = new Rect(arrowRect.xMax + 4, padded.y + 4, padded.width - 30, padded.height - 8);

            var groupOpened = EditorGUIUtility.IconContent("FolderOpened Icon");
            var groupClosed = EditorGUIUtility.IconContent("Folder Icon");
            GUI.Label(arrowRect, child.Expanded ? groupOpened : groupClosed);

            var groupDisplay = LocalizationHelper.TranslateGroup(ctx, child.Name, child.PathKey);
            GUI.Label(labelRect, groupDisplay, EditorStyles.boldLabel);

            var foldRect = new Rect(padded.x, padded.y, padded.width, padded.height);
            if (GUI.Button(foldRect, GUIContent.none, GUIStyle.none))
            {
                child.Expanded = !child.Expanded;
                FoldState.SaveFold(ctx.PreferencesKeyPrefix, child.PathKey, child.Expanded);
            }

            ExtensionRegistry.Draw(
                HookPoint.AfterGroupHeader,
                new InjectionArgs(
                    HookPoint.AfterGroupHeader, ctx, child.PathKey, null, null, depth));

            if (!child.Expanded) return;

            ExtensionRegistry.Draw(
                HookPoint.BeforeGroupContent,
                new InjectionArgs(
                    HookPoint.BeforeGroupContent, ctx, child.PathKey, null, null, depth));

            foreach (var c in child.Children.Values)
            {
                if (c.Model != null && !c.Model.Visible)
                    continue;

                DrawSingleGroup(ctx, c, depth + 1);
            }

            foreach (var prop in child.Properties)
            {
                if (prop.Model != null)
                {
                    if (!prop.Model.Visible)
                        continue;
                }
                else if (!PassSearch(ctx, prop))
                {
                    continue;
                }

                ExtensionRegistry.Draw(
                    HookPoint.BeforeProperty,
                    new InjectionArgs(
                        HookPoint.BeforeProperty, ctx, child.PathKey, prop.Property, prop.Label, depth + 1));

                var label = prop.Model != null
                    ? prop.Model.TranslatedLabel
                    : LocalizationHelper.TranslateProp(ctx, prop.Property.name, prop.Label);
                DrawPropertyRow(ctx, prop.Property, label, depth + 1);

                ExtensionRegistry.Draw(
                    HookPoint.AfterProperty,
                    new InjectionArgs(
                        HookPoint.AfterProperty, ctx, child.PathKey, prop.Property, prop.Label, depth + 1));
            }

            ExtensionRegistry.Draw(
                HookPoint.AfterGroupContent,
                new InjectionArgs(
                    HookPoint.AfterGroupContent, ctx, child.PathKey, null, null, depth));
        }

        private bool PassSearch(EditorContext ctx, TreeNodeProperty np)
        {
            if (string.IsNullOrEmpty(ctx.Search)) return true;
            var s = ctx.Search.ToLowerInvariant();
            return np.Property.name.ToLowerInvariant().Contains(s) || np.Label.ToLowerInvariant().Contains(s);
        }

        private void DrawPropertyRow(EditorContext ctx, MaterialProperty property, string label, int depth)
        {
            PropertyRowHost.Draw(ctx, property, label, depth);
        }
    }
}


