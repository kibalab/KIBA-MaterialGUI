using System;
using System.IO;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Localization;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.IO
{
    internal sealed class ShaderLocalizationFileService
    {
        public const string DefaultLocalizationFileName = "ShaderLocalization.json";
        public const string PresetFolderName = "ShaderPresets";

        public static ShaderLocalizationStore Load(string rootPath, string fileName = DefaultLocalizationFileName)
        {
            TextAsset asset = null;
            var exact = $"{rootPath}/{PresetFolderName}/{fileName}";
            if (File.Exists(exact))
            {
                asset = AssetDatabase.LoadAssetAtPath<TextAsset>(exact);
            }

            if (asset == null)
            {
                var guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName) + " t:TextAsset");
                foreach (var p in guids.Select(AssetDatabase.GUIDToAssetPath).Where(p => string.Equals(Path.GetFileName(p), fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    asset = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                    break;
                }
            }

            var store = new ShaderLocalizationStore();
            if (asset == null) return store;

            try
            {
                store = JsonUtility.FromJson<ShaderLocalizationStore>(asset.text) ?? new ShaderLocalizationStore();
                foreach (var lang in store.Languages)
                    lang?.BuildMap();
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "localization.load:" + asset.name + ":" + ex.GetType().FullName,
                    "Failed to load shader localization file '" + asset.name + "': " + ex.Message);
            }

            return store;
        }
    }
}


