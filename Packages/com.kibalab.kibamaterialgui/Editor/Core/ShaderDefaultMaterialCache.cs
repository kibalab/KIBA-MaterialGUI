#nullable enable

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    // Defaults are shader data, not per-event GUI state. This cache owns its native objects.
    [InitializeOnLoad]
    internal static class ShaderDefaultMaterialCache
    {
        private static readonly Dictionary<Shader, Material> Materials = new();
        private static int _version = -1;

        static ShaderDefaultMaterialCache()
        {
            EditorApplication.projectChanged += Clear;
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
            EditorApplication.quitting += Clear;
        }

        internal static Material? Get(Shader? shader)
        {
            if (_version != ShaderPropertyAttributeCache.Version)
            {
                Clear();
                _version = ShaderPropertyAttributeCache.Version;
            }
            if (shader == null) return null;
            if (!Materials.TryGetValue(shader, out var material) || material == null)
            {
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                Materials[shader] = material;
            }
            return material;
        }

        internal static void Clear()
        {
            foreach (var material in Materials.Values)
                if (material != null) Object.DestroyImmediate(material);
            Materials.Clear();
        }
    }
}
