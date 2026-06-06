#nullable enable

using System;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KIBA_.KIBAMaterialGUI.Editor.IO
{
    internal sealed class GradientTexturePersistenceService
    {
        private static readonly Dictionary<(int, string), GradientTextureMetadata> s_MetaCache = new();
        private static readonly Dictionary<(int, string), Texture2D> s_TexCache = new();
        private static bool s_CacheHooked;

        private static Color[]? s_PixelBuffer;

        private static void EnsureCacheHooked()
        {
            if (s_CacheHooked) return;
            s_CacheHooked = true;
            EditorApplication.projectChanged += InvalidateCache;
        }

        private static void InvalidateCache()
        {
            s_MetaCache.Clear();
            s_TexCache.Clear();
        }

        public GradientTextureMetadata LoadOrCreateMetadata(Material material, MaterialProperty property)
        {
            EnsureCacheHooked();
            var cacheKey = (material.GetInstanceID(), property.name);
            if (s_MetaCache.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;

            var path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path))
            {
                var tmp = ScriptableObject.CreateInstance<GradientTextureMetadata>();
                tmp.MaterialGuid = "MEMORY";
                tmp.PropertyName = property.name;
                return tmp;
            }

            var meta = FindSubAsset<GradientTextureMetadata>(path, m => m.PropertyName == property.name);
            if (meta != null)
            {
                s_MetaCache[cacheKey] = meta;
                return meta;
            }

            meta = ScriptableObject.CreateInstance<GradientTextureMetadata>();
            meta.MaterialGuid = AssetDatabase.AssetPathToGUID(path);
            meta.PropertyName = property.name;
            meta.name = $"{property.name}_GradientMeta";
            AssetDatabase.AddObjectToAsset(meta, material);
            EditorUtility.SetDirty(meta);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            s_MetaCache[cacheKey] = meta;
            return meta;
        }

        public Texture2D LoadOrCreateTexture(Material material, MaterialProperty property, GradientTextureMetadata meta)
        {
            EnsureCacheHooked();
            var cacheKey = (material.GetInstanceID(), property.name);
            if (s_TexCache.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;

            var path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path))
            {
                var texMem = new Texture2D(meta.Width, meta.Height, TextureFormat.RGBA32, false, true)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    name = $"{property.name}_GradientTex"
                };
                return texMem;
            }

            var tex = FindSubAsset<Texture2D>(path, t => t.name == $"{property.name}_GradientTex");
            if (tex != null)
            {
                s_TexCache[cacheKey] = tex;
                return tex;
            }

            tex = new Texture2D(meta.Width, meta.Height, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = $"{property.name}_GradientTex"
            };
            AssetDatabase.AddObjectToAsset(tex, material);
            EditorUtility.SetDirty(tex);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            s_TexCache[cacheKey] = tex;
            return tex;
        }

        public static void BakeIntoTexture(GradientTextureMetadata meta, Texture2D tex)
        {
            BakePixels(meta, tex);
            EditorUtility.SetDirty(tex);
            AssetDatabase.SaveAssets();
        }

        public static void BakePixelsOnly(GradientTextureMetadata meta, Texture2D tex)
        {
            BakePixels(meta, tex);
        }

        private static void BakePixels(GradientTextureMetadata meta, Texture2D tex)
        {
#if UNITY_2021_2_OR_NEWER
            if (tex.width != meta.Width || tex.height != meta.Height)
                tex.Reinitialize(meta.Width, meta.Height, TextureFormat.RGBA32, false);
#endif
            var size = meta.Width * meta.Height;
            if (s_PixelBuffer == null || s_PixelBuffer.Length != size)
                s_PixelBuffer = new Color[size];

            var pixels = s_PixelBuffer;
            for (int x = 0; x < meta.Width; x++)
            {
                var c = meta.Gradient.Evaluate((float)x / (meta.Width - 1));
                for (int y = 0; y < meta.Height; y++)
                    pixels[y * meta.Width + x] = c;
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
        }

        public static void AssignTextureToMaterial(Material material, MaterialProperty property, Texture2D tex)
        {
            if (property.type != MaterialProperty.PropType.Texture) return;
            property.textureValue = tex;
            EditorUtility.SetDirty(material);
        }

        private static T? FindSubAsset<T>(string assetPath, Predicate<T>? predicate) where T : Object
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var o in all)
            {
                if (o is T t && (predicate == null || predicate(t))) return t;
            }

            return null;
        }
    }
}


