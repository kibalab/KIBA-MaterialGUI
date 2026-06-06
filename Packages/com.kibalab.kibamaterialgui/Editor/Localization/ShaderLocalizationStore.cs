#nullable enable

using System;
using System.Collections.Generic;

namespace KIBA_.KIBAMaterialGUI.Editor.Localization
{
    [Serializable]
    internal class ShaderLocalizationStore
    {
        public List<ShaderLanguageTable> Languages = new();
        public string DefaultCode = "EN";
        [NonSerialized] private Dictionary<string, ShaderLanguageTable>? _tableByCode;
        [NonSerialized] private ShaderLanguageTable? _defaultTable;
        [NonSerialized] private int _indexedLanguageCount = -1;

        public string Get(string lang, string key, string? fallback = null)
        {
            if (Languages.Count == 0) return fallback ?? key;

            EnsureTableIndex();

            ShaderLanguageTable? table = null;
            if (!string.IsNullOrWhiteSpace(lang) &&
                _tableByCode != null &&
                _tableByCode.TryGetValue(lang, out var foundByLang))
            {
                table = foundByLang;
            }
            else if (_defaultTable != null)
            {
                table = _defaultTable;
            }
            else if (Languages.Count > 0)
            {
                table = Languages[0];
            }

            if (table == null) return fallback ?? key;
            if (table.Map == null || table.Map.Count == 0) table.BuildMap();
            var map = table.Map;
            if (map == null) return fallback ?? key;
            var skey = ShaderLocalizationUtil.SanitizeKey(key);
            return map.TryGetValue(skey, out var v) ? v : (fallback ?? key);
        }

        private void EnsureTableIndex()
        {
            if (_tableByCode != null && _indexedLanguageCount == Languages.Count)
                return;

            _tableByCode = new Dictionary<string, ShaderLanguageTable>(StringComparer.OrdinalIgnoreCase);
            _defaultTable = null;
            _indexedLanguageCount = Languages.Count;

            for (var i = 0; i < Languages.Count; i++)
            {
                var table = Languages[i];
                if (table == null) continue;
                if (!string.IsNullOrWhiteSpace(table.Code))
                    _tableByCode[table.Code] = table;

                if (_defaultTable == null &&
                    !string.IsNullOrWhiteSpace(DefaultCode) &&
                    string.Equals(table.Code, DefaultCode, StringComparison.OrdinalIgnoreCase))
                {
                    _defaultTable = table;
                }
            }
        }
    }
}


