#nullable enable

using System;
using System.Collections.Generic;

namespace KIBA_.KIBAMaterialGUI.Editor.Localization
{
    [Serializable]
    internal class ShaderLanguageTable
    {
        public string Code = "EN";
        public List<ShaderLocKv> Entries = new();
        [NonSerialized] public Dictionary<string, string> Map = null!;

        public void BuildMap()
        {
            Map = new Dictionary<string, string>();
            foreach (var kv in Entries)
            {
                if (kv == null || string.IsNullOrEmpty(kv.Key)) continue;

                var key = ShaderLocalizationUtil.SanitizeKey(kv.Key);
                Map[key] = kv.Value ?? "";
            }
        }
    }
}


