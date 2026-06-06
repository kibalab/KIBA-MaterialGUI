#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.IO
{
    internal sealed class ShaderPresetFileService
    {
        public const string DefaultPresetFileName = "ShaderPresets.json";
        public const string PresetFolderName = "ShaderPresets";

        public static ShaderPresetStore Load(string rootPath, string fileName = DefaultPresetFileName)
        {
            TextAsset? asset = null;
            var exact = $"{rootPath}/{PresetFolderName}/{fileName}";
            if (File.Exists(exact)) asset = AssetDatabase.LoadAssetAtPath<TextAsset>(exact);
            if (asset == null)
            {
                var guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName) + " t:TextAsset");
                foreach (var g in guids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    if (Path.GetFileName(p).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        asset = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                        break;
                    }
                }
            }

            if (asset == null) return new ShaderPresetStore();

            try
            {
                return JsonUtility.FromJson<ShaderPresetStore>(asset.text) ?? new ShaderPresetStore();
            }
            catch
            {
                return new ShaderPresetStore();
            }
        }

        public static void Save(string rootPath, ShaderPresetStore store, string fileName = DefaultPresetFileName)
        {
            var dir = Path.Combine(rootPath, PresetFolderName);
            var path = Path.Combine(dir, fileName);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonUtility.ToJson(store, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        public static void UpsertPresetEntry(ShaderPresetStore store, string shaderFullName, PresetEntry entry)
        {
            var regex = "^" + Regex.Escape(shaderFullName) + "$";
            var group = store.Groups?.FirstOrDefault(g => g.ShaderNameRegex == regex);
            if (group == null)
            {
                group = new PresetGroup { ShaderNameRegex = regex, Presets = new List<PresetEntry>() };
                store.Groups ??= new List<PresetGroup>();
                store.Groups.Add(group);
            }

            group.Presets ??= new List<PresetEntry>();
            var exist = group.Presets.FirstOrDefault(p => string.Equals(p.Name, entry.Name, StringComparison.OrdinalIgnoreCase));
            if (exist != null)
            {
                exist.Values = entry.Values;
            }
            else group.Presets.Add(entry);
        }

        public static List<PresetEntry> GetMatchedPresets(ShaderPresetStore? store, string shaderName)
        {
            var matched = new List<PresetEntry>();
            if (store?.Groups == null) return matched;

            foreach (var g in store.Groups)
            {
                var ok = true;
                if (!string.IsNullOrEmpty(g.ShaderNameRegex))
                {
                    try
                    {
                        ok = Regex.IsMatch(shaderName ?? string.Empty, g.ShaderNameRegex);
                    }
                    catch
                    {
                        ok = false;
                    }
                }

                if (!ok) continue;

                if (g.Presets != null) matched.AddRange(g.Presets);
            }

            return matched;
        }
    }
}


