#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal static class FoldState
    {
        private static readonly Dictionary<string, bool> Memory = new();

        public static int Version { get; private set; }

        private static string Key(string prefix, string key) => $"{prefix}Fold.{key}";

        public static bool GetFold(string prefix, string key, bool defVal)
        {
            var fullKey = Key(prefix, key);
            if (Memory.TryGetValue(fullKey, out var v)) return v;
            if (EditorPrefs.HasKey(fullKey))
            {
                var pv = EditorPrefs.GetBool(fullKey, defVal);
                Memory[fullKey] = pv;
                return pv;
            }

            Memory[fullKey] = defVal;
            return defVal;
        }

        public static void SaveFold(string prefix, string key, bool val)
        {
            var fullKey = Key(prefix, key);
            Memory[fullKey] = val;
            EditorPrefs.SetBool(fullKey, val);
        }

        public static void SetAllFolds(string prefix, bool open)
        {
            var keys = Memory.Keys.ToList();
            foreach (var k in keys)
            {
                Memory[k] = open;
                EditorPrefs.SetBool(k, open);
            }

            Version++;
        }
    }
}


