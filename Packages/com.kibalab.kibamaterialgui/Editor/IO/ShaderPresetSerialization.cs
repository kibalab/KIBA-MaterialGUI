#nullable enable

using System;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.IO
{
    internal static class ShaderPresetSerialization
    {
        internal static ShaderPresetStore ReadStore(string json)
        {
            var legacy = JsonUtility.FromJson<LegacyStore>(json);
            if (legacy?.Groups == null || legacy.Groups.Count == 0)
                return JsonUtility.FromJson<ShaderPresetStore>(json) ?? new ShaderPresetStore();

            var store = new ShaderPresetStore();
            foreach (var source in legacy.Groups)
            {
                if (source == null) continue;
                var group = new PresetGroup { ShaderNameRegex = source.ShaderNameRegex ?? ".*" };
                if (source.Presets != null)
                    foreach (var entry in source.Presets)
                        if (entry != null) group.Presets.Add(Convert(entry));
                store.Groups.Add(group);
            }
            return store;
        }

        internal static PresetEntry ReadEntry(string json)
        {
            var legacy = JsonUtility.FromJson<LegacyEntry>(json);
            return legacy != null && (!string.IsNullOrEmpty(legacy.Name) || legacy.Values is { Count: > 0 })
                ? Convert(legacy)
                : JsonUtility.FromJson<PresetEntry>(json);
        }

        private static PresetEntry Convert(LegacyEntry source)
        {
            var entry = new PresetEntry { Name = source.Name ?? "Preset" };
            if (source.Values == null) return entry;
            foreach (var value in source.Values)
            {
                if (value == null) continue;
                entry.Values.Add(new PresetValue
                {
                    Property = value.Property ?? string.Empty,
                    Float = value.Float,
                    Vector4 = value.Vector4 ?? new float[4],
                    Color = value.Color ?? new float[4],
                    TextureGuid = value.TextureGuid ?? string.Empty
                });
            }
            return entry;
        }

        // JsonUtility does not honor FormerlySerializedAs when reading these old JSON keys.
        // These DTOs deliberately retain the compiler-generated field names of format v1.
        [Serializable] private sealed class LegacyStore
        {
            [field: SerializeField] public List<LegacyGroup>? Groups { get; set; }
        }
        [Serializable] private sealed class LegacyGroup
        {
            [field: SerializeField] public string? ShaderNameRegex { get; set; }
            [field: SerializeField] public List<LegacyEntry>? Presets { get; set; }
        }
        [Serializable] private sealed class LegacyEntry
        {
            [field: SerializeField] public string? Name { get; set; }
            [field: SerializeField] public List<LegacyValue>? Values { get; set; }
        }
        [Serializable] private sealed class LegacyValue
        {
            [field: SerializeField] public string? Property { get; set; }
            [field: SerializeField] public float Float { get; set; }
            [field: SerializeField] public float[]? Vector4 { get; set; }
            [field: SerializeField] public float[]? Color { get; set; }
            [field: SerializeField] public string? TextureGuid { get; set; }
        }
    }
}
