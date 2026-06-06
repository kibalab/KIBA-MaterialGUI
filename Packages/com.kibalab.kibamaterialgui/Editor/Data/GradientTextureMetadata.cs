#nullable enable

using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Data
{
    internal sealed class GradientTextureMetadata : ScriptableObject
    {
        public string MaterialGuid = string.Empty;
        public string PropertyName = string.Empty;
        public int Width = 64;
        public int Height = 1;

        public Gradient Gradient = new()
        {
            colorKeys = new[]
            {
                new GradientColorKey(Color.black, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };
    }
}


