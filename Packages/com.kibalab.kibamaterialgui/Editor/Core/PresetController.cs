#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using KIBA_.KIBAMaterialGUI.Editor.IO;
using KIBA_.KIBAMaterialGUI.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    internal class PresetController
    {
        private static PresetEntry? _clipboard;

        private string? _cachedPresetsShaderName;

        public void BuildMatchedPresets(EditorContext ctx)
        {
            var shaderName = ctx.Material.shader != null ? ctx.Material.shader.name : string.Empty;
            if (shaderName == _cachedPresetsShaderName) return;
            _cachedPresetsShaderName = shaderName;
            ctx.MatchedPresets = ShaderPresetFileService.GetMatchedPresets(ctx.PresetStore, shaderName);
        }

        public void ApplyPreset(EditorContext ctx, PresetEntry preset)
        {
            if (preset == null || preset.Values == null) return;
            var dict = new Dictionary<string, MaterialProperty>();
            foreach (var p in ctx.Properties) dict[p.name] = p;
            ctx.MaterialEditor.RegisterPropertyChangeUndo("Apply Preset " + preset.Name);
            foreach (var v in preset.Values.Where(v => !string.IsNullOrEmpty(v.Property)))
            {
                if (!dict.TryGetValue(v.Property, out var mp)) continue;
                switch (mp.type)
                {
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        mp.floatValue = v.Float; break;
                    case MaterialProperty.PropType.Color:
                        if (v.Color is { Length: >= 3 })
                        {
                            var a = v.Color.Length >= 4 ? v.Color[3] : 1f;
                            mp.colorValue = new Color(v.Color[0], v.Color[1], v.Color[2], a);
                        }

                        break;
                    case MaterialProperty.PropType.Vector:
                        if (v.Vector4 is { Length: >= 4 }) mp.vectorValue = new Vector4(v.Vector4[0], v.Vector4[1], v.Vector4[2], v.Vector4[3]);
                        break;
                    case MaterialProperty.PropType.Texture:
                        if (!string.IsNullOrEmpty(v.TextureGuid))
                        {
                            var path = AssetDatabase.GUIDToAssetPath(v.TextureGuid);
                            var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                            if (tex != null) mp.textureValue = tex;
                        }

                        break;
                }
            }
        }

        public void ApplyPresetToProps(EditorContext ctx, PresetEntry preset, List<MaterialProperty> targetProps)
        {
            if (preset == null || preset.Values == null) return;
            var dict = new Dictionary<string, MaterialProperty>();
            foreach (var p in targetProps) dict[p.name] = p;
            ctx.MaterialEditor.RegisterPropertyChangeUndo("Apply Group Paste");
            foreach (var v in preset.Values.Where(v => !string.IsNullOrEmpty(v.Property)))
            {
                if (!dict.TryGetValue(v.Property, out var mp)) continue;
                switch (mp.type)
                {
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        mp.floatValue = v.Float; break;
                    case MaterialProperty.PropType.Color:
                        if (v.Color is { Length: >= 3 })
                        {
                            var a = v.Color.Length >= 4 ? v.Color[3] : 1f;
                            mp.colorValue = new Color(v.Color[0], v.Color[1], v.Color[2], a);
                        }

                        break;
                    case MaterialProperty.PropType.Vector:
                        if (v.Vector4 is { Length: >= 4 }) mp.vectorValue = new Vector4(v.Vector4[0], v.Vector4[1], v.Vector4[2], v.Vector4[3]);
                        break;
                    case MaterialProperty.PropType.Texture:
                        if (!string.IsNullOrEmpty(v.TextureGuid))
                        {
                            var path = AssetDatabase.GUIDToAssetPath(v.TextureGuid);
                            var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                            if (tex != null) mp.textureValue = tex;
                        }

                        break;
                }
            }
        }

        public PresetEntry CapturePreset(EditorContext ctx, string name, List<MaterialProperty> props)
        {
            var vals = new List<PresetValue>();
            foreach (var p in props)
            {
                var v = new PresetValue { Property = p.name };
                switch (p.type)
                {
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        v.Float = p.floatValue; break;
                    case MaterialProperty.PropType.Color:
                        var c = p.colorValue;
                        v.Color = new[] { c.r, c.g, c.b, c.a };
                        break;
                    case MaterialProperty.PropType.Vector:
                        var vec = p.vectorValue;
                        v.Vector4 = new[] { vec.x, vec.y, vec.z, vec.w };
                        break;
                    case MaterialProperty.PropType.Texture:
                        var tex = p.textureValue;
                        if (tex != null)
                        {
                            var path = AssetDatabase.GetAssetPath(tex);
                            v.TextureGuid = AssetDatabase.AssetPathToGUID(path);
                        }

                        break;
                }

                vals.Add(v);
            }

            var entry = new PresetEntry { Name = name, Values = vals };
            _clipboard = entry;
            return entry;
        }

        public void CopyPresetJsonToClipboard(EditorContext ctx, string name, List<MaterialProperty> props)
        {
            var entry = CapturePreset(ctx, name, props);
            var json = JsonUtility.ToJson(entry, true);
            EditorGUIUtility.systemCopyBuffer = json;
        }

        public void PromptPresetNameAndSave(EditorContext ctx, string suggestedName, List<MaterialProperty> props)
        {
            string tTitle = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:NewPresetTitle", "New Preset") ?? "New Preset";
            string tLabel = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:Name", "Name") ?? "Name";
            string tOk = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:Create", "Create") ?? "Create";
            string tCancel = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:Cancel", "Cancel") ?? "Cancel";

            PresetNamePromptWindow.Open(
                tTitle, tLabel, suggestedName, tOk, tCancel,
                (enteredName) =>
                {
                    var final = string.IsNullOrWhiteSpace(enteredName) ? suggestedName : enteredName.Trim();
                    AddPresetFromCurrentToFile(ctx, final, props);
                }
            );
        }

        private void AddPresetFromCurrentToFile(EditorContext ctx, string presetName, List<MaterialProperty> props)
        {
            var finalName = string.IsNullOrWhiteSpace(presetName)
                ? $"{ctx.Material.name}_Preset_{DateTime.Now:yyyyMMdd_HHmmss}"
                : presetName.Trim();

            var entry = CapturePreset(ctx, finalName, props);

            var exist =
                ShaderPresetFileService.GetMatchedPresets(ctx.PresetStore, ctx.Material.shader != null ? ctx.Material.shader.name : string.Empty)
                    .FirstOrDefault(p => string.Equals(p.Name, finalName, System.StringComparison.OrdinalIgnoreCase));

            if (exist != null)
            {
                var tTitle = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:OverwriteTitle", "Preset Exists") ?? "Preset Exists";
                var tMsg = (ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:OverwriteMsg", "A preset with the same name already exists. Overwrite?")
                            ?? "A preset with the same name already exists. Overwrite?")
                           + $"\n\n\"{finalName}\"";
                var tOk = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:Overwrite", "Overwrite") ?? "Overwrite";
                var tCancel = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:Cancel", "Cancel") ?? "Cancel";

                if (!EditorUtility.DisplayDialog(tTitle, tMsg, tOk, tCancel)) return;
            }

            if (ctx.Material.shader == null) return;

            ShaderPresetFileService.UpsertPresetEntry(ctx.PresetStore, ctx.Material.shader.name, entry);
            ShaderPresetFileService.Save(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ctx.Material.shader)) ?? "Assets",
                ctx.PresetStore, ShaderPresetFileService.DefaultPresetFileName);
        }

        public void SetClipboard(PresetEntry e) => _clipboard = e;
        public bool HasClipboard() => _clipboard != null;
        public PresetEntry? GetClipboard() => _clipboard;
    }
}


