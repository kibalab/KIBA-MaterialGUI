#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    public static class ShaderPropertyAttributeCache
    {
        [Flags]
        public enum UiHintFlags
        {
            None = 0,
            GradientTexture = 1 << 0,
            FlexibleRange = 1 << 1,
            Divider = 1 << 2,
            SegmentedEnum = 1 << 3,
            MinMaxSlider = 1 << 4,
        }

        public struct EnumInfo
        {
            public string[] names;
            public float[] values;

            public string[] Names => names ?? Array.Empty<string>();
            public float[] Values => values ?? Array.Empty<float>();
        }

        public struct ToggleInfo
        {
            public bool found;
            public bool invert;
            public string? keyword;

            public bool Found => found;
            public bool Invert => invert;
            public string? Keyword => keyword;
        }

        public readonly struct ShaderAttributeInfo
        {
            public readonly string name;
            public readonly string args;
            public readonly string raw;

            public string Name => name;
            public string Args => args;
            public string Raw => raw;

            public ShaderAttributeInfo(string name, string args, string raw)
            {
                this.name = name ?? string.Empty;
                this.args = args ?? string.Empty;
                this.raw = raw ?? string.Empty;
            }
        }

        private static readonly Dictionary<string, Dictionary<string, EnumInfo>> s_EnumCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, ToggleInfo>> s_ToggleCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, string[]>> s_KeywordEnumCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, string>> s_GroupPathCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, UiHintFlags>> s_UiHintCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, float>> s_TopSpacePxCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, ShaderAttributeInfo[]>> s_AttributeCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, int>> s_VectorFieldCountCache = new(StringComparer.Ordinal);

        private static bool s_ProjectChangedHooked;

        private static readonly Dictionary<string, EnumInfo> s_KnownEnums = new(StringComparer.Ordinal)
        {
            ["UnityEngine.Rendering.CullMode"] = MakeEnumFromType(typeof(UnityEngine.Rendering.CullMode)),
            ["CullMode"] = MakeEnumFromType(typeof(UnityEngine.Rendering.CullMode)),
            ["UnityEngine.Rendering.BlendMode"] = MakeEnumFromType(typeof(UnityEngine.Rendering.BlendMode)),
            ["BlendMode"] = MakeEnumFromType(typeof(UnityEngine.Rendering.BlendMode)),
        };

        public static bool TryGetEnum(Shader? shader, string propName, out EnumInfo info)
        {
            info = default;
            if (!EnsureBuilt(shader, out var key)) return false;
            return s_EnumCache.TryGetValue(key, out var map) && map.TryGetValue(propName, out info);
        }

        public static bool TryGetToggle(Shader? shader, string propName, out ToggleInfo info)
        {
            info = default;
            if (!EnsureBuilt(shader, out var key)) return false;
            return s_ToggleCache.TryGetValue(key, out var map) && map.TryGetValue(propName, out info);
        }

        public static bool TryGetKeywordEnum(Shader? shader, string propName, out string[] options)
        {
            options = Array.Empty<string>();
            if (!EnsureBuilt(shader, out var key)) return false;
            if (!s_KeywordEnumCache.TryGetValue(key, out var map) || !map.TryGetValue(propName, out var found) || found == null || found.Length == 0)
                return false;
            options = found;
            return true;
        }

        public static bool TryGetGroupPath(Shader? shader, string propName, out string groupPath)
        {
            groupPath = string.Empty;
            if (!EnsureBuilt(shader, out var key)) return false;
            return s_GroupPathCache.TryGetValue(key, out var map) && map.TryGetValue(propName, out groupPath);
        }

        public static bool HasUiHint(Shader? shader, string propName, UiHintFlags hint)
        {
            if (!EnsureBuilt(shader, out var key)) return false;
            return s_UiHintCache.TryGetValue(key, out var map) &&
                   map.TryGetValue(propName, out var flags) &&
                   (flags & hint) != 0;
        }

        public static bool HasDivider(Shader? shader, string propName)
        {
            return HasUiHint(shader, propName, UiHintFlags.Divider);
        }

        public static bool TryGetTopSpacePx(Shader? shader, string propName, out float px)
        {
            px = 0f;
            if (!EnsureBuilt(shader, out var key)) return false;
            if (!s_TopSpacePxCache.TryGetValue(key, out var map) || !map.TryGetValue(propName, out var found))
                return false;

            px = Mathf.Max(0f, found);
            return true;
        }

        public static bool TryGetShaderAttributes(Shader? shader, string propName, out IReadOnlyList<ShaderAttributeInfo> attributes)
        {
            attributes = Array.Empty<ShaderAttributeInfo>();
            if (!EnsureBuilt(shader, out var key)) return false;
            if (!s_AttributeCache.TryGetValue(key, out var map) || !map.TryGetValue(propName, out var found) || found == null || found.Length == 0)
                return false;

            attributes = found;
            return true;
        }

        public static bool HasShaderAttribute(Shader? shader, string propName, string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName)) return false;
            return TryGetShaderAttribute(shader, propName, attributeName, out _);
        }

        public static bool TryGetShaderAttribute(Shader? shader, string propName, string attributeName, out ShaderAttributeInfo attribute)
        {
            attribute = default;
            if (string.IsNullOrWhiteSpace(attributeName)) return false;
            if (!TryGetShaderAttributes(shader, propName, out var attrs)) return false;

            for (int i = 0; i < attrs.Count; i++)
            {
                var a = attrs[i];
                if (!string.Equals(a.name, attributeName, StringComparison.OrdinalIgnoreCase)) continue;
                attribute = a;
                return true;
            }

            return false;
        }

        public static bool TryGetVectorFieldCount(Shader? shader, string propName, out int count)
        {
            count = 4;
            if (!EnsureBuilt(shader, out var key)) return false;
            if (!s_VectorFieldCountCache.TryGetValue(key, out var map) || !map.TryGetValue(propName, out var found))
                return false;

            count = Mathf.Clamp(found, 2, 4);
            return true;
        }

        private static EnumInfo MakeEnumFromType(Type t)
        {
            var names = Enum.GetNames(t);
            var valuesObj = Enum.GetValues(t);
            var values = new float[names.Length];
            for (int i = 0; i < names.Length; i++)
                values[i] = Convert.ToSingle(valuesObj.GetValue(i), CultureInfo.InvariantCulture);
            return new EnumInfo { names = names, values = values };
        }

        private static bool EnsureBuilt(Shader? shader, out string key)
        {
            key = string.Empty;
            if (shader == null) return false;

            var path = AssetDatabase.GetAssetPath(shader);
            if (string.IsNullOrEmpty(path)) return false;

            key = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(key)) key = path;

            if (s_AttributeCache.ContainsKey(key))
                return true;

            var enumMap = new Dictionary<string, EnumInfo>(StringComparer.Ordinal);
            var toggleMap = new Dictionary<string, ToggleInfo>(StringComparer.Ordinal);
            var keywordEnumMap = new Dictionary<string, string[]>(StringComparer.Ordinal);
            var groupPathMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var uiHintMap = new Dictionary<string, UiHintFlags>(StringComparer.Ordinal);
            var topSpacePxMap = new Dictionary<string, float>(StringComparer.Ordinal);
            var attributeMap = new Dictionary<string, List<ShaderAttributeInfo>>(StringComparer.Ordinal);
            var vectorFieldCountMap = new Dictionary<string, int>(StringComparer.Ordinal);

            BuildFromShaderApi(shader, enumMap, toggleMap, keywordEnumMap, groupPathMap, uiHintMap, topSpacePxMap, attributeMap, vectorFieldCountMap);

            var compactAttributeMap = new Dictionary<string, ShaderAttributeInfo[]>(StringComparer.Ordinal);
            foreach (var kv in attributeMap)
            {
                if (kv.Value == null || kv.Value.Count == 0) continue;
                compactAttributeMap[kv.Key] = kv.Value.ToArray();
            }

            s_EnumCache[key] = enumMap;
            s_ToggleCache[key] = toggleMap;
            s_KeywordEnumCache[key] = keywordEnumMap;
            s_GroupPathCache[key] = groupPathMap;
            s_UiHintCache[key] = uiHintMap;
            s_TopSpacePxCache[key] = topSpacePxMap;
            s_AttributeCache[key] = compactAttributeMap;
            s_VectorFieldCountCache[key] = vectorFieldCountMap;

            HookProjectChangedOnce();
            return true;
        }

        private static void BuildFromShaderApi(
            Shader shader,
            Dictionary<string, EnumInfo> enumMap,
            Dictionary<string, ToggleInfo> toggleMap,
            Dictionary<string, string[]> keywordEnumMap,
            Dictionary<string, string> groupPathMap,
            Dictionary<string, UiHintFlags> uiHintMap,
            Dictionary<string, float> topSpacePxMap,
            Dictionary<string, List<ShaderAttributeInfo>> attributeMap,
            Dictionary<string, int> vectorFieldCountMap)
        {
            int propertyCount;
            try
            {
                propertyCount = shader.GetPropertyCount();
            }
            catch
            {
                return;
            }

            for (var i = 0; i < propertyCount; i++)
            {
                string propName;
                string[] attrs;
                ShaderPropertyType propType;
                string propDescription;
                try
                {
                    propName = shader.GetPropertyName(i);
                    propType = shader.GetPropertyType(i);
                    attrs = shader.GetPropertyAttributes(i) ?? Array.Empty<string>();
                    propDescription = shader.GetPropertyDescription(i);
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(propName)) continue;

                foreach (var raw in attrs)
                {
                    if (!TrySplitAttribute(raw, out var attrName, out var args)) continue;
                    if (string.IsNullOrWhiteSpace(attrName)) continue;

                    var key = attrName.Trim();
                    var argText = args ?? string.Empty;
                    var rawText = raw ?? string.Empty;

                    if (!attributeMap.TryGetValue(propName, out var attrList))
                    {
                        attrList = new List<ShaderAttributeInfo>();
                        attributeMap[propName] = attrList;
                    }

                    attrList.Add(new ShaderAttributeInfo(key, argText, rawText));
                    ApplyKnownAttributeSemantics(
                        propName,
                        key,
                        argText,
                        enumMap,
                        toggleMap,
                        keywordEnumMap,
                        groupPathMap,
                        uiHintMap,
                        topSpacePxMap,
                        vectorFieldCountMap);
                }

                ApplyLegacyBuiltInConventions(propName, propDescription, propType, enumMap, toggleMap);
            }
        }

        private static void ApplyKnownAttributeSemantics(
            string propName,
            string key,
            string args,
            Dictionary<string, EnumInfo> enumMap,
            Dictionary<string, ToggleInfo> toggleMap,
            Dictionary<string, string[]> keywordEnumMap,
            Dictionary<string, string> groupPathMap,
            Dictionary<string, UiHintFlags> uiHintMap,
            Dictionary<string, float> topSpacePxMap,
            Dictionary<string, int> vectorFieldCountMap)
        {
            if (key.Equals("Enum", StringComparison.OrdinalIgnoreCase))
            {
                var inside = TrimQuotes((args ?? string.Empty).Trim());
                if (TryMakeEnumInfo(inside, out var einfo))
                    enumMap[propName] = einfo;
                return;
            }

            if (key.Equals("KeywordEnum", StringComparison.OrdinalIgnoreCase))
            {
                var options = ParseKeywordEnumOptions(args ?? string.Empty);
                if (options.Length > 0)
                    keywordEnumMap[propName] = options;
                return;
            }

            if (key.Equals("Toggle", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("MaterialToggle", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("ToggleOff", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("ToggleUI", StringComparison.OrdinalIgnoreCase))
            {
                var kw = TrimQuotes((args ?? string.Empty).Trim());
                toggleMap[propName] = new ToggleInfo
                {
                    found = true,
                    invert = key.Equals("ToggleOff", StringComparison.OrdinalIgnoreCase),
                    keyword = string.IsNullOrWhiteSpace(kw) ? null : ToKeywordToken(kw)
                };
                return;
            }

            if (key.Equals("Group", StringComparison.OrdinalIgnoreCase))
            {
                var path = ParseGroupPath(args ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(path))
                    groupPathMap[propName] = path;
                return;
            }

            if (key.Equals("Space", StringComparison.OrdinalIgnoreCase))
            {
                if (TryParseSpacePx(args ?? string.Empty, out var px))
                    topSpacePxMap[propName] = px;
                return;
            }

            if (key.Equals("Divider", StringComparison.OrdinalIgnoreCase))
            {
                AddHint(uiHintMap, propName, UiHintFlags.Divider);
                return;
            }

            if (key.Equals("GradientTexture", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Gradient", StringComparison.OrdinalIgnoreCase))
            {
                AddHint(uiHintMap, propName, UiHintFlags.GradientTexture);
                return;
            }

            if (key.Equals("FlexibleRange", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Flexible", StringComparison.OrdinalIgnoreCase))
            {
                AddHint(uiHintMap, propName, UiHintFlags.FlexibleRange);
                return;
            }

            if (key.Equals("SegmentedEnum", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Segmented", StringComparison.OrdinalIgnoreCase))
            {
                AddHint(uiHintMap, propName, UiHintFlags.SegmentedEnum);
                return;
            }

            if (key.Equals("MinMaxSlider", StringComparison.OrdinalIgnoreCase))
            {
                AddHint(uiHintMap, propName, UiHintFlags.MinMaxSlider);
                return;
            }

            if (key.Equals("Vector2", StringComparison.OrdinalIgnoreCase))
            {
                vectorFieldCountMap[propName] = 2;
                return;
            }

            if (key.Equals("Vector3", StringComparison.OrdinalIgnoreCase))
            {
                vectorFieldCountMap[propName] = 3;
                return;
            }

            if (key.Equals("Vector4", StringComparison.OrdinalIgnoreCase))
            {
                vectorFieldCountMap[propName] = 4;
                return;
            }

            if (key.Equals("Vector", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("VectorN", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("VectorFields", StringComparison.OrdinalIgnoreCase))
            {
                if (TryParseVectorFieldCount(args ?? string.Empty, out var count))
                    vectorFieldCountMap[propName] = count;
            }
        }

        private static void HookProjectChangedOnce()
        {
            if (s_ProjectChangedHooked) return;
            s_ProjectChangedHooked = true;
            EditorApplication.projectChanged += InvalidateAll;
        }

        public static void InvalidateAll()
        {
            s_EnumCache.Clear();
            s_ToggleCache.Clear();
            s_KeywordEnumCache.Clear();
            s_GroupPathCache.Clear();
            s_UiHintCache.Clear();
            s_TopSpacePxCache.Clear();
            s_AttributeCache.Clear();
            s_VectorFieldCountCache.Clear();
        }

        private static bool TryMakeEnumInfo(string inside, out EnumInfo info)
        {
            info = default;
            if (string.IsNullOrWhiteSpace(inside)) return false;

            if (s_KnownEnums.TryGetValue(inside, out info))
                return true;

            var typeCandidates = new[]
            {
                inside,
                $"{inside}, UnityEngine.CoreModule",
                $"{inside}, UnityEngine",
                $"{inside}, Assembly-CSharp"
            };
            foreach (var cand in typeCandidates)
            {
                var t = Type.GetType(cand, false);
                if (t != null && t.IsEnum)
                {
                    info = MakeEnumFromType(t);
                    return true;
                }
            }

            var tokens = SplitTopLevelCsv(inside)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToArray();

            if (tokens.Length < 2 || (tokens.Length % 2) != 0) return false;

            var names = new List<string>(tokens.Length / 2);
            var values = new List<float>(tokens.Length / 2);
            for (int i = 0; i < tokens.Length; i += 2)
            {
                names.Add(TrimQuotes(tokens[i]));
                values.Add(ParseFloat(tokens[i + 1]));
            }

            info = new EnumInfo { names = names.ToArray(), values = values.ToArray() };
            return true;
        }

        private static string[] ParseKeywordEnumOptions(string inside)
        {
            var tokens = SplitTopLevelCsv(inside)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s =>
                {
                    var eq = s.IndexOf('=');
                    return eq >= 0 ? s.Substring(0, eq).Trim() : s;
                })
                .Select(TrimQuotes)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            return tokens;
        }

        private static bool TrySplitAttribute(string raw, out string name, out string args)
        {
            name = string.Empty;
            args = string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            var text = raw.Trim();
            if (text.StartsWith("[", StringComparison.Ordinal) && text.EndsWith("]", StringComparison.Ordinal) && text.Length > 2)
                text = text.Substring(1, text.Length - 2).Trim();

            if (string.IsNullOrWhiteSpace(text)) return false;

            var open = text.IndexOf('(');
            if (open < 0)
            {
                name = text;
                return true;
            }

            var close = text.LastIndexOf(')');
            if (close < open) close = text.Length;
            name = text.Substring(0, open).Trim();
            args = close > open ? text.Substring(open + 1, close - open - 1).Trim() : string.Empty;
            return true;
        }

        private static IEnumerable<string> SplitTopLevelCsv(string text)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            var start = 0;
            var depth = 0;
            var inSingle = false;
            var inDouble = false;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (inSingle)
                {
                    if (c == '\'') inSingle = false;
                    else if (c == '\\') i++;
                    continue;
                }

                if (inDouble)
                {
                    if (c == '"') inDouble = false;
                    else if (c == '\\') i++;
                    continue;
                }

                if (c == '\'')
                {
                    inSingle = true;
                    continue;
                }

                if (c == '"')
                {
                    inDouble = true;
                    continue;
                }

                if (c == '(')
                {
                    depth++;
                    continue;
                }

                if (c == ')')
                {
                    if (depth > 0) depth--;
                    continue;
                }

                if (c != ',' || depth != 0) continue;

                yield return text.Substring(start, i - start);
                start = i + 1;
            }

            if (start <= text.Length)
                yield return text.Substring(start);
        }

        private static string NormalizeGroupPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            return TrimQuotes(path.Trim());
        }

        internal static string ParseGroupPath(string args)
        {
            if (string.IsNullOrWhiteSpace(args)) return string.Empty;

            var tokens = SplitTopLevelCsv(args)
                .Select(t => TrimQuotes(t.Trim()))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();

            if (tokens.Length == 0) return string.Empty;
            if (tokens.Any(ContainsUnsupportedGroupPathSeparator)) return string.Empty;
            if (tokens.Length == 1) return NormalizeGroupPath(tokens[0]);

            return string.Join("/", tokens.Select(NormalizeGroupPath).Where(t => t.Length > 0));
        }

        private static bool ContainsUnsupportedGroupPathSeparator(string token)
        {
            return token.IndexOf('/') >= 0 ||
                   token.IndexOf('\\') >= 0 ||
                   token.IndexOf('／') >= 0;
        }

        private static bool TryParseVectorFieldCount(string args, out int count)
        {
            count = 4;
            if (string.IsNullOrWhiteSpace(args)) return false;

            var first = SplitTopLevelCsv(args)
                .Select(t => TrimQuotes(t.Trim()))
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
            if (string.IsNullOrWhiteSpace(first)) return false;

            var v = ParseFloat(first);
            var n = Mathf.RoundToInt(v);
            if (n < 2 || n > 4) return false;

            count = n;
            return true;
        }

        private static bool TryParseSpacePx(string args, out float px)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                px = EditorGUIUtility.singleLineHeight;
                return true;
            }

            var first = SplitTopLevelCsv(args)
                .Select(t => TrimQuotes(t.Trim()))
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

            if (string.IsNullOrWhiteSpace(first))
            {
                px = EditorGUIUtility.singleLineHeight;
                return true;
            }

            if (first.EndsWith("px", StringComparison.OrdinalIgnoreCase))
                first = first.Substring(0, first.Length - 2).Trim();

            px = Mathf.Max(0f, ParseFloat(first));
            return true;
        }

        private static void ApplyLegacyBuiltInConventions(
            string propName,
            string propDescription,
            ShaderPropertyType propType,
            Dictionary<string, EnumInfo> enumMap,
            Dictionary<string, ToggleInfo> toggleMap)
        {
            if (string.IsNullOrWhiteSpace(propName)) return;
            if (propType != ShaderPropertyType.Float && propType != ShaderPropertyType.Range) return;

            if (!enumMap.ContainsKey(propName) && IsLikelyCullProperty(propName, propDescription))
            {
                enumMap[propName] = s_KnownEnums["UnityEngine.Rendering.CullMode"];
            }

            if (!toggleMap.ContainsKey(propName) &&
                !enumMap.ContainsKey(propName) &&
                IsLikelyDoubleSidedToggle(propName, propDescription))
            {
                toggleMap[propName] = new ToggleInfo
                {
                    found = true,
                    invert = false,
                    keyword = null
                };
            }
        }

        private static bool IsLikelyCullProperty(string propName, string propDescription)
        {
            var n = (propName ?? string.Empty).Trim().ToLowerInvariant();
            var d = (propDescription ?? string.Empty).Trim().ToLowerInvariant();

            if (n == "_cull" || n == "cull" ||
                n == "_cullmode" || n == "cullmode" ||
                n == "_culling" || n == "culling")
            {
                return true;
            }

            if ((n.EndsWith("_cull", StringComparison.Ordinal) || n.EndsWith("_cullmode", StringComparison.Ordinal)) &&
                n.IndexOf("occlusion", StringComparison.Ordinal) < 0)
            {
                return true;
            }

            return d.IndexOf("cull mode", StringComparison.Ordinal) >= 0;
        }

        private static bool IsLikelyDoubleSidedToggle(string propName, string propDescription)
        {
            var n = (propName ?? string.Empty).Trim().ToLowerInvariant();
            var d = (propDescription ?? string.Empty).Trim().ToLowerInvariant();

            if (n.IndexOf("doublesided", StringComparison.Ordinal) >= 0 ||
                n.IndexOf("double_sided", StringComparison.Ordinal) >= 0 ||
                n.IndexOf("twosided", StringComparison.Ordinal) >= 0 ||
                n.IndexOf("two_sided", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            if (d.IndexOf("double sided", StringComparison.Ordinal) >= 0 ||
                d.IndexOf("two sided", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }

        private static void AddHint(Dictionary<string, UiHintFlags> map, string propertyName, UiHintFlags hint)
        {
            map.TryGetValue(propertyName, out var cur);
            map[propertyName] = cur | hint;
        }

        private static float ParseFloat(string s)
        {
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var iv))
                    return iv;
            }

            float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f);
            return f;
        }

        private static string TrimQuotes(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Trim();
            if (s.Length >= 2)
            {
                if ((s[0] == '"' && s[^1] == '"') || (s[0] == '\'' && s[^1] == '\''))
                    return s.Substring(1, s.Length - 2).Trim();
            }

            return s;
        }

        private static string ToKeywordToken(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            raw = raw.ToUpperInvariant();
            var ch = raw.ToCharArray();
            for (int i = 0; i < ch.Length; i++)
            {
                char c = ch[i];
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_') continue;
                ch[i] = '_';
            }

            return new string(ch);
        }
    }
}


