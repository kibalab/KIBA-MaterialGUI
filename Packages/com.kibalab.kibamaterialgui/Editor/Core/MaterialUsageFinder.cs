#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    internal enum MaterialUsageKind
    {
        SceneObject,
        Prefab
    }

    internal class MaterialUsageItem
    {
        public MaterialUsageKind Kind;
        public string Display = "";
        public string? SceneName;
        public string? AssetPath;
        public Object? ObjectRef;
    }

    internal static class MaterialUsageFinder
    {
        public static List<MaterialUsageItem> FindInOpenScenes(Material material)
        {
            var list = new List<MaterialUsageItem>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (!sc.isLoaded) continue;

                var roots = sc.GetRootGameObjects();
                foreach (var r in roots)
                {
                    var renderers = r.GetComponentsInChildren<Renderer>(true);
                    foreach (var rend in renderers)
                    {
                        if (!RendererUsesMaterial(rend, material)) continue;

                        var go = rend.gameObject;
                        list.Add(new MaterialUsageItem
                        {
                            Kind = MaterialUsageKind.SceneObject,
                            ObjectRef = go,
                            SceneName = sc.name,
                            Display = $"{GetHierarchyPath(go)}"
                        });
                    }
                }
            }

            return list;
        }

        public static List<MaterialUsageItem> FindInProjectPrefabs(Material material)
        {
            var list = new List<MaterialUsageItem>();
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root == null) continue;

                var renderers = root.GetComponentsInChildren<Renderer>(true);
                var added = false;
                foreach (var rend in renderers.Where(rend => RendererUsesMaterial(rend, material)))
                {
                    list.Add(new MaterialUsageItem
                    {
                        Kind = MaterialUsageKind.Prefab,
                        AssetPath = path,
                        Display = $"{root.name} / {BuildLocalPath(root, rend.gameObject)}"
                    });
                    added = true;
                    break;
                }

                if (added) continue;

                var ps = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
                foreach (var pr in ps.Where(pr => RendererUsesMaterial(pr, material)))
                {
                    list.Add(new MaterialUsageItem
                    {
                        Kind = MaterialUsageKind.Prefab,
                        AssetPath = path,
                        Display = $"{root.name} / {BuildLocalPath(root, pr.gameObject)}"
                    });
                    break;
                }
            }

            return list;
        }

        private static bool RendererUsesMaterial(Renderer r, Material target)
        {
            var mats = r.sharedMaterials;
            return Enumerable.OfType<Material>(mats).Any(t => ReferenceEquals(t, target));
        }

        private static string GetHierarchyPath(GameObject go)
        {
            var stack = new Stack<string>();
            var t = go.transform;
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }

            return string.Join("/", stack);
        }

        private static string BuildLocalPath(GameObject root, GameObject child)
        {
            if (root == child) return child.name;

            var names = new List<string>();
            var t = child.transform;
            while (t != null && t.gameObject != root)
            {
                names.Add(t.name);
                t = t.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}


