#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Data
{
    [Serializable]
    internal class ShaderPresetStore
    {
        public List<PresetGroup> Groups = new();
    }

    [Serializable]
    internal class PresetGroup
    {
        public string ShaderNameRegex = ".*";
        public List<PresetEntry> Presets = new();
    }

    [Serializable]
    internal class PresetEntry
    {
        public int FormatVersion = 2;
        public string Name = "Preset";
        public List<PresetValue> Values = new();
    }

    [Serializable]
    internal class PresetValue
    {
        public string Property = string.Empty;
        public float Float;
        public int Integer;
        public float[] Vector4 = new float[4];
        public float[] Color = new float[4];
        public string TextureGuid = string.Empty;
        public bool HasTextureValue;
        public bool TextureIsNull;
        public long TextureLocalId;
        public float[]? TextureScaleOffset;
        [NonSerialized] public Texture? SessionTexture;
    }
}


