#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Data
{
    [Serializable]
    internal class ShaderPresetStore
    {
        [field: SerializeField] public List<PresetGroup> Groups { get; set; } = new();
    }

    [Serializable]
    internal class PresetGroup
    {
        [field: SerializeField] public string ShaderNameRegex { get; set; } = ".*";
        [field: SerializeField] public List<PresetEntry> Presets { get; set; } = new();
    }

    [Serializable]
    internal class PresetEntry
    {
        [field: SerializeField] public string Name { get; set; } = "Preset";
        [field: SerializeField] public List<PresetValue> Values { get; set; } = new();
    }

    [Serializable]
    internal class PresetValue
    {
        [field: SerializeField] public string Property { get; set; } = string.Empty;
        [field: SerializeField] public float Float { get; set; }
        [field: SerializeField] public float[] Vector4 { get; set; } = new float[4];
        [field: SerializeField] public float[] Color { get; set; } = new float[4];
        [field: SerializeField] public string TextureGuid { get; set; } = string.Empty;
    }
}


