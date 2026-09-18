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
        private ShaderPresetStore? _cachedStore;
        private List<PresetEntry>? _cachedPresets;

        public void BuildMatchedPresets(EditorContext ctx)
        {
            var shaderName = ctx.Material.shader != null ? ctx.Material.shader.name : string.Empty;
            if (shaderName != _cachedPresetsShaderName || !ReferenceEquals(_cachedStore, ctx.PresetStore) || _cachedPresets == null)
            {
                _cachedPresetsShaderName = shaderName;
                _cachedStore = ctx.PresetStore;
                _cachedPresets = ShaderPresetFileService.GetMatchedPresets(ctx.PresetStore, shaderName);
            }
            ctx.MatchedPresets = _cachedPresets;
        }

        public void ApplyPreset(EditorContext ctx, PresetEntry preset)
        {
            if (preset == null) return;
            ApplyValues(ctx, preset, ctx.Properties, "Apply Preset " + preset.Name);
        }

        public void ApplyPresetToProps(EditorContext ctx, PresetEntry preset, List<MaterialProperty> targetProps)
        {
            ApplyValues(ctx, preset, targetProps, "Apply Group Paste");
        }

        private static void ApplyValues(EditorContext ctx, PresetEntry preset, IReadOnlyList<MaterialProperty> targetProps, string undoName)
        {
            if (preset == null || preset.Values == null) return;
            var dict = new Dictionary<string, MaterialProperty>();
            foreach (var p in targetProps) dict[p.name] = p;
            ctx.MaterialEditor?.RegisterPropertyChangeUndo(undoName);
            foreach (var v in preset.Values.Where(v => !string.IsNullOrEmpty(v.Property)))
            {
                if (!dict.TryGetValue(v.Property, out var mp)) continue;
                switch (mp.type)
                {
                    case MaterialProperty.PropType.Int:
                        mp.intValue = v.Integer; break;
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
                        if (v.HasTextureValue && v.TextureIsNull)
                            mp.textureValue = null;
                        else if (v.SessionTexture != null)
                            mp.textureValue = v.SessionTexture;
                        else if (!string.IsNullOrEmpty(v.TextureGuid))
                        {
                            var path = AssetDatabase.GUIDToAssetPath(v.TextureGuid);
                            Texture? tex = null;
                            if (v.TextureLocalId == 0) tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                            else foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                            {
                                if (asset is Texture candidate && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(candidate, out string _, out long id) && id == v.TextureLocalId)
                                { tex = candidate; break; }
                            }
                            if (tex != null) mp.textureValue = tex;
                        }
                        if (v.TextureScaleOffset is { Length: >= 4 })
                            mp.textureScaleAndOffset = new Vector4(v.TextureScaleOffset[0], v.TextureScaleOffset[1], v.TextureScaleOffset[2], v.TextureScaleOffset[3]);

                        break;
                }
            }
            GUI.changed = true;
            ctx.MaterialEditor?.PropertiesChanged();
            ctx.MaterialEditor?.Repaint();
        }

        public PresetEntry CapturePreset(EditorContext ctx, string name, List<MaterialProperty> props)
        {
            var vals = new List<PresetValue>();
            foreach (var p in props)
            {
                var v = new PresetValue { Property = p.name };
                switch (p.type)
                {
                    case MaterialProperty.PropType.Int:
                        v.Integer = p.intValue; break;
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
                        v.HasTextureValue = true;
                        v.TextureIsNull = tex == null;
                        v.SessionTexture = tex;
                        var st = p.textureScaleAndOffset;
                        v.TextureScaleOffset = new[] { st.x, st.y, st.z, st.w };
                        if (tex != null)
                        {
                            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tex, out string guid, out long localId))
                            {
                                v.TextureGuid = guid;
                                v.TextureLocalId = localId;
                            }
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
            _cachedPresets = null;
            ShaderPresetFileService.Save(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ctx.Material.shader)) ?? "Assets",
                ctx.PresetStore, ShaderPresetFileService.DefaultPresetFileName);
        }

        public void SetClipboard(PresetEntry e) => _clipboard = e;
        public bool HasClipboard() => _clipboard != null;
        public PresetEntry? GetClipboard() => _clipboard;
    }
}


