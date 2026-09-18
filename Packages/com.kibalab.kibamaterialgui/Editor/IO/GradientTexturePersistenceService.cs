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
        private static readonly Dictionary<int, Material> s_TransientOwners = new();
        private static readonly List<int> s_DeadOwners = new();
        private static readonly HashSet<Object> s_PendingSaves = new();
        private static double s_NextCleanup;
        private static double s_SaveAfter;

        private static Color[]? s_PixelBuffer;

        private static void EnsureCacheHooked()
        {
            if (s_CacheHooked) return;
            s_CacheHooked = true;
            EditorApplication.projectChanged += InvalidateCache;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
            EditorApplication.quitting += Clear;
        }

        private static void InvalidateCache()
        {
            foreach (var owner in new List<Material>(s_TransientOwners.Values))
                if (owner != null && EditorUtility.IsPersistent(owner)) Promote(owner);
            RemovePersistentEntries(s_MetaCache);
            RemovePersistentEntries(s_TexCache);
        }

        private static void Promote(Material material)
        {
            var id = material.GetInstanceID();
            if (!s_TransientOwners.ContainsKey(id) || !EditorUtility.IsPersistent(material)) return;
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
            foreach (var entry in s_MetaCache)
            {
                if (entry.Key.Item1 != id || entry.Value == null || EditorUtility.IsPersistent(entry.Value)) continue;
                entry.Value.MaterialGuid = guid;
                entry.Value.hideFlags = HideFlags.None;
                AssetDatabase.AddObjectToAsset(entry.Value, material);
                EditorUtility.SetDirty(entry.Value);
            }
            foreach (var entry in s_TexCache)
            {
                if (entry.Key.Item1 != id || entry.Value == null || EditorUtility.IsPersistent(entry.Value)) continue;
                entry.Value.hideFlags = HideFlags.None;
                AssetDatabase.AddObjectToAsset(entry.Value, material);
                EditorUtility.SetDirty(entry.Value);
            }
            s_TransientOwners.Remove(id);
            EditorUtility.SetDirty(material);
            QueueSave(material);
        }

        private static void RemovePersistentEntries<T>(Dictionary<(int, string), T> cache) where T : Object
        {
            var keys = new List<(int, string)>();
            foreach (var entry in cache)
                if (!s_TransientOwners.ContainsKey(entry.Key.Item1)) keys.Add(entry.Key);
            foreach (var key in keys) cache.Remove(key);
        }

        internal static void Release(Material material)
        {
            if (ReferenceEquals(material, null)) return;
            Release(material.GetInstanceID());
        }

        private static void Release(int id)
        {
            ReleaseEntries(s_MetaCache, id);
            ReleaseEntries(s_TexCache, id);
            s_TransientOwners.Remove(id);
        }

        private static void ReleaseEntries<T>(Dictionary<(int, string), T> cache, int id) where T : Object
        {
            var keys = new List<(int, string)>();
            foreach (var entry in cache)
            {
                if (entry.Key.Item1 != id) continue;
                if (entry.Value != null && !EditorUtility.IsPersistent(entry.Value))
                    Object.DestroyImmediate(entry.Value);
                keys.Add(entry.Key);
            }
            foreach (var key in keys) cache.Remove(key);
        }

        internal static void Clear()
        {
            FlushSaves();
            foreach (var id in new List<int>(s_TransientOwners.Keys)) Release(id);
            s_MetaCache.Clear();
            s_TexCache.Clear();
        }

        private static void Update()
        {
            var now = EditorApplication.timeSinceStartup;
            if (s_PendingSaves.Count > 0 && now >= s_SaveAfter) FlushSaves();
            if (now < s_NextCleanup) return;
            s_NextCleanup = now + 1;
            s_DeadOwners.Clear();
            foreach (var owner in s_TransientOwners)
                if (owner.Value == null) s_DeadOwners.Add(owner.Key);
            foreach (var id in s_DeadOwners) Release(id);
        }

        private static void QueueSave(Object asset)
        {
            if (!EditorUtility.IsPersistent(asset)) return;
            s_PendingSaves.Add(asset);
            s_SaveAfter = EditorApplication.timeSinceStartup + 0.3;
        }

        private static void FlushSaves()
        {
            if (s_PendingSaves.Count == 0) return;
            var assets = new Object[s_PendingSaves.Count];
            s_PendingSaves.CopyTo(assets);
            s_PendingSaves.Clear();
            foreach (var asset in assets)
                if (asset != null) AssetDatabase.SaveAssetIfDirty(asset);
        }

        public GradientTextureMetadata LoadOrCreateMetadata(Material material, MaterialProperty property)
        {
            EnsureCacheHooked();
            var cacheKey = (material.GetInstanceID(), property.name);
            if (s_MetaCache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                if (s_TransientOwners.ContainsKey(cacheKey.Item1)) Promote(material);
                return cached;
            }

            var path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path))
            {
                var tmp = ScriptableObject.CreateInstance<GradientTextureMetadata>();
                tmp.MaterialGuid = "MEMORY";
                tmp.PropertyName = property.name;
                tmp.name = $"{property.name}_GradientMeta";
                tmp.hideFlags = HideFlags.HideAndDontSave;
                s_TransientOwners[material.GetInstanceID()] = material;
                s_MetaCache[cacheKey] = tmp;
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
            QueueSave(material);

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
                    name = $"{property.name}_GradientTex",
                    hideFlags = HideFlags.HideAndDontSave
                };
                s_TransientOwners[material.GetInstanceID()] = material;
                s_TexCache[cacheKey] = texMem;
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
            QueueSave(material);

            s_TexCache[cacheKey] = tex;
            return tex;
        }

        public static void BakeIntoTexture(GradientTextureMetadata meta, Texture2D tex)
        {
            BakePixels(meta, tex);
            EditorUtility.SetDirty(tex);
            QueueSave(tex);
            QueueSave(meta);
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


