#nullable enable

using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using KIBA_.KIBAMaterialGUI.Editor.IO;
using KIBA_.KIBAMaterialGUI.Editor.Localization;
using KIBA_.KIBAMaterialGUI.Editor.Parsing;
using KIBA_.KIBAMaterialGUI.Editor.UI;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    public class MaterialGUIContext
    {
        private Material[] _targets = System.Array.Empty<Material>();
        private MaterialProperty[] _properties = System.Array.Empty<MaterialProperty>();
        private IReadOnlyList<Material> _targetsView = System.Array.Empty<Material>();
        private IReadOnlyList<MaterialProperty> _propertiesView = System.Array.Empty<MaterialProperty>();

        internal MaterialGUIContext()
        {
        }

        public MaterialEditor MaterialEditor { get; internal set; } = null!;
        public Material Material { get; internal set; } = null!;
        public IReadOnlyList<Material> Targets
        {
            get => _targetsView;
            internal set
            {
                _targets = ToArray(value);
                _targetsView = System.Array.AsReadOnly(_targets);
            }
        }

        public IReadOnlyList<MaterialProperty> Properties
        {
            get => _propertiesView;
            internal set
            {
                _properties = ToArray(value);
                _propertiesView = System.Array.AsReadOnly(_properties);
            }
        }

        public string CurrentLanguage { get; internal set; } = string.Empty;
        public string Search { get; internal set; } = string.Empty;
        public MaterialGUIModel Model { get; internal set; } = null!;

        internal Material[] TargetArray => _targets;
        internal MaterialProperty[] PropertyArray => _properties;

        private static T[] ToArray<T>(IReadOnlyList<T>? source)
        {
            if (source == null || source.Count == 0)
                return System.Array.Empty<T>();

            if (source is T[] array)
                return array;

            var copy = new T[source.Count];
            for (var i = 0; i < source.Count; i++)
                copy[i] = source[i];
            return copy;
        }
    }

    internal sealed class EditorContext : MaterialGUIContext
    {
        public IMaterialPropertyDisplayParser DisplayParser = null!;
        public ShaderPresetFileService PresetFileService = null!;
        public ShaderLocalizationFileService LocalizationFileService = null!;
        public MaterialPropertyRendererRegistry RendererRegistry = null!;
        public MaterialGUIStyles Styles = null!;

        public ShaderPresetStore PresetStore = null!;
        public ShaderLocalizationStore LocalizationStore = null!;

        public MaterialGUIState State = new();

        public List<PresetEntry> MatchedPresets = new();

        public string PreferencesKeyPrefix = string.Empty;
        public string PreferencesLanguageKey = string.Empty;
    }

    internal static class MaterialGUIInternalDiagnostics
    {
        public const string VerboseEditorPrefKey = "KIBA_.KIBAMaterialGUI.Diagnostics.Verbose";

        private static readonly HashSet<string> s_Warned = new();

        public static bool VerboseEnabled
        {
            get => EditorPrefs.GetBool(VerboseEditorPrefKey, false);
            set => EditorPrefs.SetBool(VerboseEditorPrefKey, value);
        }

        public static void WarnOnce(string key, string message)
        {
            if (!VerboseEnabled) return;
            if (string.IsNullOrEmpty(key)) key = message;
            if (!s_Warned.Add(key)) return;

            Debug.LogWarning("[KIBAMaterialGUI] " + message);
        }

#if UNITY_INCLUDE_TESTS
        internal static void ResetForTests()
        {
            s_Warned.Clear();
        }
#endif
    }
}


