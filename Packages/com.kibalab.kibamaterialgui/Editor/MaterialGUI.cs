#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace KIBA_.KIBAMaterialGUI.Editor
{
    public class MaterialGUI : ShaderGUI
    {
        private const string PreferencesKeyPrefix = "KIBA_.KIBAMaterialGUI.";
        private const string PreferencesLanguageKey = PreferencesKeyPrefix + "Lang";

        private static bool s_UndoHooked;
        private static int s_UndoVersion;
        private static bool s_AnimationRepaintHooked;
        private static readonly System.Collections.Generic.List<MaterialEditor> s_AnimationPreviewEditors = new();
        private static readonly MethodInfo? InAnimationRecordingMethod = typeof(AnimationMode).GetMethod(
            "InAnimationRecording",
            BindingFlags.NonPublic | BindingFlags.Static);

        private MaterialEditor _materialEditor = null!;
        private Material _material = null!;
        private MaterialProperty[] _properties = null!;

        private IMaterialPropertyDisplayParser? _displayParser;
        private ShaderPresetFileService? _presetFileService;
        private ShaderLocalizationFileService? _localizationFileService;
        private MaterialPropertyRendererRegistry? _rendererRegistry;

        private ShaderPresetStore? _presetStore;
        private ShaderLocalizationStore? _localizationStore;

        private MaterialGUIStyles? _styles;
        private ToolbarView? _toolbar;
        private HeaderView? _header;
        private GroupTreeView? _tree;
        private FooterView? _footer;
        private PresetController? _presets;
        private RenderQueueController? _renderQueue;
        private MaterialGUIState? _state;

        private EditorContext? _ctx;

        private UnityEngine.Object[]? _cachedTargetsSource;
        private Material[]? _cachedTargets;

        private string? _cachedLanguage;

        private TreeNode? _cachedRoot;
        private Shader? _cachedTreeShader;
        private int _cachedFoldVersion = -1;
        private int _cachedStateVersion = -1;
        private int _cachedPropertiesSignature;
        private int _seenUndoVersion = -1;

        private string RootPath => (_material != null && _material.shader != null
            ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(_material.shader))
            : "Assets") ?? string.Empty;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            EnsureUndoHooked();

            _materialEditor = materialEditor;
            if (_materialEditor.target is not Material primaryMaterial) return;

            if (ConsumeUndoSignal())
            {
                InvalidateFrameCaches();

                try
                {
                    _materialEditor.PropertiesChanged();
                    var refreshed = MaterialEditor.GetMaterialProperties(_materialEditor.targets);
                    if (refreshed is { Length: > 0 })
                        props = refreshed;
                }
                catch (Exception ex)
                {
                    MaterialGUIInternalDiagnostics.WarnOnce(
                        "shader-gui.undo-refresh:" + ex.GetType().FullName,
                        "Failed to refresh material properties after Undo/Redo: " + ex.Message);
                }
            }

            if (AnimationMode.InAnimationMode() && !IsAnimationRecording())
                RefreshAnimationPreviewProperties(ref props);

            _material = primaryMaterial;
            _properties = props;

            _displayParser ??= new MaterialPropertyDisplayParser();
            _presetFileService ??= new ShaderPresetFileService();
            _localizationFileService ??= new ShaderLocalizationFileService();
            _rendererRegistry ??= new MaterialPropertyRendererRegistry();

            _presetStore ??= ShaderPresetFileService.Load(RootPath);
            _localizationStore ??= ShaderLocalizationFileService.Load(RootPath);
            var localizationStore = _localizationStore;

            _styles ??= new MaterialGUIStyles();
            _styles.Ensure();

            var defaultLanguage = localizationStore.DefaultCode ?? "EN";
            var prefsLanguage = EditorPrefs.GetString(PreferencesLanguageKey, defaultLanguage);
            if (!string.Equals(_cachedLanguage, prefsLanguage, StringComparison.Ordinal))
                _cachedLanguage = prefsLanguage;
            if (string.IsNullOrWhiteSpace(_cachedLanguage))
                _cachedLanguage = defaultLanguage;
            var currentLanguage = _cachedLanguage ?? defaultLanguage;

            if (_ctx == null)
            {
                _ctx = new EditorContext
                {
                    DisplayParser = _displayParser,
                    PresetFileService = _presetFileService,
                    LocalizationFileService = _localizationFileService,
                    RendererRegistry = _rendererRegistry,
                    PreferencesKeyPrefix = PreferencesKeyPrefix,
                    PreferencesLanguageKey = PreferencesLanguageKey,
                };
            }

            _ctx.MaterialEditor = _materialEditor;
            _ctx.Material = _material;
            _ctx.Targets = ResolveTargets();
            _ctx.Properties = _properties;
            _ctx.PresetStore = _presetStore;
            _ctx.LocalizationStore = localizationStore;
            _ctx.CurrentLanguage = currentLanguage;
            _ctx.Styles = _styles;

            _presets ??= new PresetController();
            _renderQueue ??= new RenderQueueController();
            _state ??= new MaterialGUIState();
            _header ??= new HeaderView();
            _toolbar ??= new ToolbarView(_presets, _renderQueue);
            _tree ??= new GroupTreeView(_presets);
            _footer ??= new FooterView();

            _ctx.State = _state;
            _ctx.Search = _state.Search;

            _presets.BuildMatchedPresets(_ctx);

            ExtensionRegistry.Draw(HookPoint.BeforeHeader, new InjectionArgs(HookPoint.BeforeHeader, _ctx));
            _header.Draw(_ctx);
            ExtensionRegistry.Draw(HookPoint.AfterHeader, new InjectionArgs(HookPoint.AfterHeader, _ctx));

            ExtensionRegistry.Draw(HookPoint.BeforeToolbar, new InjectionArgs(HookPoint.BeforeToolbar, _ctx));
            _toolbar.Draw(_ctx);
            ExtensionRegistry.Draw(HookPoint.AfterToolbar, new InjectionArgs(HookPoint.AfterToolbar, _ctx));

            EditorGUILayout.Space(8);

            ExtensionRegistry.Draw(HookPoint.BeforeTree, new InjectionArgs(HookPoint.BeforeTree, _ctx));
            var root = GetOrBuildTree(_ctx);
            _tree.Draw(_ctx, root);
            ExtensionRegistry.Draw(HookPoint.AfterTree, new InjectionArgs(HookPoint.AfterTree, _ctx));

            ExtensionRegistry.Draw(HookPoint.BeforeFooter, new InjectionArgs(HookPoint.BeforeFooter, _ctx));
            _footer.Draw(_ctx);
            ExtensionRegistry.Draw(HookPoint.AfterFooter, new InjectionArgs(HookPoint.AfterFooter, _ctx));

            _ctx.Search = _state.Search;

            if (!string.Equals(_cachedLanguage, _ctx.CurrentLanguage, StringComparison.Ordinal))
            {
                _cachedLanguage = _ctx.CurrentLanguage;
                EditorPrefs.SetString(PreferencesLanguageKey, _cachedLanguage);
            }

            if (GUI.changed)
                InvalidateFrameCaches();
        }

        private TreeNode GetOrBuildTree(EditorContext ctx)
        {
            var shader = _material?.shader;
            var foldVer = FoldState.Version;
            var stateVer = ctx.State?.Version ?? 0;
            var propSig = ComputePropertiesSignature(ctx.Properties);
            if (_cachedRoot != null
                && ReferenceEquals(_cachedTreeShader, shader)
                && _cachedFoldVersion == foldVer
                && _cachedStateVersion == stateVer
                && _cachedPropertiesSignature == propSig)
                return _cachedRoot;

            _cachedRoot = TreeBuilder.Build(ctx);
            _cachedTreeShader = shader;
            _cachedFoldVersion = foldVer;
            _cachedStateVersion = stateVer;
            _cachedPropertiesSignature = propSig;
            return _cachedRoot;
        }

        private static int ComputePropertiesSignature(IReadOnlyList<MaterialProperty> properties)
        {
            unchecked
            {
                var hash = 17;
                if (properties == null) return hash;
                hash = hash * 31 + properties.Count;
                for (var i = 0; i < properties.Count; i++)
                {
                    var p = properties[i];
                    if (p == null) continue;
                    hash = hash * 31 + (p.name != null ? p.name.GetHashCode() : 0);
                    hash = hash * 31 + (p.displayName != null ? p.displayName.GetHashCode() : 0);
                    hash = hash * 31 + (int)p.type;
                }

                return hash;
            }
        }

        private Material[] ResolveTargets()
        {
            var source = _materialEditor.targets;
            if (_cachedTargets != null && ReferenceEquals(source, _cachedTargetsSource))
                return _cachedTargets;

            _cachedTargetsSource = source;
            _cachedTargets = source?
                .OfType<Material>()
                .Where(static m => m != null)
                .Distinct()
                .ToArray() ?? Array.Empty<Material>();
            if (_cachedTargets.Length == 0)
                _cachedTargets = new[] { _material };
            return _cachedTargets;
        }

        private static void EnsureUndoHooked()
        {
            if (s_UndoHooked) return;
            s_UndoHooked = true;
            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
        }

        private static void HandleUndoRedoPerformed()
        {
            unchecked
            {
                s_UndoVersion++;
            }
        }

        private void RefreshAnimationPreviewProperties(ref MaterialProperty[] props)
        {
            RegisterAnimationPreviewRepaint(_materialEditor);
            InvalidateFrameCaches();

            try
            {
                var refreshed = MaterialEditor.GetMaterialProperties(_materialEditor.targets);
                if (refreshed is { Length: > 0 })
                {
                    MaterialEditor.PrepareMaterialPropertiesForAnimationMode(refreshed, true);
                    props = refreshed;
                }
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "shader-gui.animation-refresh:" + ex.GetType().FullName,
                    "Failed to refresh animated material properties: " + ex.Message);
            }
        }

        private static void RegisterAnimationPreviewRepaint(MaterialEditor editor)
        {
            if (editor == null) return;

            if (!s_AnimationPreviewEditors.Contains(editor))
                s_AnimationPreviewEditors.Add(editor);

            if (s_AnimationRepaintHooked) return;
            s_AnimationRepaintHooked = true;
            EditorApplication.update += RepaintAnimationPreviewEditors;
        }

        private static void RepaintAnimationPreviewEditors()
        {
            if (!AnimationMode.InAnimationMode() || IsAnimationRecording())
            {
                s_AnimationPreviewEditors.Clear();
                if (s_AnimationRepaintHooked)
                {
                    s_AnimationRepaintHooked = false;
                    EditorApplication.update -= RepaintAnimationPreviewEditors;
                }

                return;
            }

            for (var i = s_AnimationPreviewEditors.Count - 1; i >= 0; i--)
            {
                var editor = s_AnimationPreviewEditors[i];
                if (editor == null)
                {
                    s_AnimationPreviewEditors.RemoveAt(i);
                    continue;
                }

                editor.Repaint();
            }
        }

        private static bool IsAnimationRecording()
        {
            if (!AnimationMode.InAnimationMode()) return false;
            if (InAnimationRecordingMethod == null) return false;

            try
            {
                return InAnimationRecordingMethod.Invoke(null, null) is true;
            }
            catch
            {
                return false;
            }
        }

        private bool ConsumeUndoSignal()
        {
            if (_seenUndoVersion < 0)
            {
                _seenUndoVersion = s_UndoVersion;
                return false;
            }

            if (_seenUndoVersion == s_UndoVersion)
                return false;

            _seenUndoVersion = s_UndoVersion;
            return true;
        }

        private void InvalidateFrameCaches()
        {
            _cachedRoot = null;
            _cachedTreeShader = null;
            _cachedFoldVersion = -1;
            _cachedStateVersion = -1;
            _cachedPropertiesSignature = 0;

            _cachedTargetsSource = null;
            _cachedTargets = null;
        }
    }
}




