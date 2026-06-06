#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    internal static class ShaderSupportUtil
    {
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int CullMode = Shader.PropertyToID("_CullMode");
        private static readonly int Culling = Shader.PropertyToID("_Culling");
        private static readonly int DoubleSidedEnable = Shader.PropertyToID("_DoubleSidedEnable");
        private static readonly Dictionary<int, bool> s_InstancingSupportCache = new();
        private static bool s_ProjectChangedHooked;

        public static bool GetDoubleSided(Material m)
        {
            if (m == null) return false;
            if (m.HasProperty(Cull)) return Mathf.Approximately(m.GetFloat(Cull), 0f);
            if (m.HasProperty(CullMode)) return Mathf.Approximately(m.GetFloat(CullMode), 0f);
            if (m.HasProperty(Culling)) return Mathf.Approximately(m.GetFloat(Culling), 0f);
            if (m.HasProperty(DoubleSidedEnable)) return m.GetFloat(DoubleSidedEnable) > 0.5f;

            return false;
        }

        public static void SetDoubleSided(Material m, bool v)
        {
            if (m == null) return;

            if (m.HasProperty(Cull)) m.SetFloat(Cull, v ? 0f : 2f);
            if (m.HasProperty(CullMode)) m.SetFloat(CullMode, v ? 0f : 2f);
            if (m.HasProperty(Culling)) m.SetFloat(Culling, v ? 0f : 2f);
            if (m.HasProperty(DoubleSidedEnable)) m.SetFloat(DoubleSidedEnable, v ? 1f : 0f);
        }

        public static bool DetectDoubleSidedSupport(Material m)
        {
            if (m == null) return false;

            return m.HasProperty(Cull) || m.HasProperty(CullMode) || m.HasProperty(Culling) || m.HasProperty(DoubleSidedEnable);
        }

        public static bool DetectInstancingSupport(Shader shader)
        {
            if (shader == null) return false;
            HookProjectChangedOnce();

            var shaderId = shader.GetInstanceID();
            if (s_InstancingSupportCache.TryGetValue(shaderId, out var cached))
                return cached;

            var result = ComputeInstancingSupport(shader);
            s_InstancingSupportCache[shaderId] = result;
            return result;
        }

        private static bool ComputeInstancingSupport(Shader shader)
        {
            if (shader == null) return false;

            var path = AssetDatabase.GetAssetPath(shader);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return true;

            try
            {
                var txt = File.ReadAllText(path);
                if (txt.IndexOf("multi_compile_instancing", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (txt.IndexOf("instancing_options", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            catch (Exception ex)
            {
                MaterialGUIInternalDiagnostics.WarnOnce(
                    "shader-support.instancing-source:" + ex.GetType().FullName,
                    "Failed to inspect shader source for instancing support: " + ex.Message);
            }

            return true;
        }

        private static void HookProjectChangedOnce()
        {
            if (s_ProjectChangedHooked) return;
            s_ProjectChangedHooked = true;
            EditorApplication.projectChanged += ClearCaches;
        }

        private static void ClearCaches()
        {
            s_InstancingSupportCache.Clear();
        }
    }
}


