#nullable enable

using System;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Parsing
{
    internal interface IMaterialPropertyDisplayParser
    {
        void ParseDisplay(string displayName, out string path, out string label);
    }

    internal sealed class MaterialPropertyDisplayParser : IMaterialPropertyDisplayParser
    {
        private static bool s_WarnedLegacyPathDsl;
        private static bool s_WarnedLegacyHintDsl;

        public void ParseDisplay(string displayName, out string path, out string label)
        {
            path = string.Empty;
            label = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();

            if (LooksLikeLegacyPathDsl(displayName))
            {
                WarnLegacyPathDslOnce();
                label = StripLegacyPathPrefix(label);
            }

            if (!ContainsLegacyHintDsl(label)) return;

            WarnLegacyHintDslOnce();
            label = RemoveLegacyHintTokens(label);
        }

        private static bool LooksLikeLegacyPathDsl(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return false;

            var text = displayName.TrimStart();
            if (!text.StartsWith("[", StringComparison.Ordinal)) return false;
            var end = text.IndexOf(']');
            return end > 1;
        }

        private static string StripLegacyPathPrefix(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var trimmed = text.TrimStart();
            if (!trimmed.StartsWith("[", StringComparison.Ordinal)) return text.Trim();

            var end = trimmed.IndexOf(']');
            if (end <= 1) return text.Trim();

            return trimmed[(end + 1)..].TrimStart();
        }

        private static bool ContainsLegacyHintDsl(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return text.IndexOf("{gradient}", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("{flexible}", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string RemoveLegacyHintTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            return text
                .Replace("{gradient}", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("{flexible}", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();
        }

        private static void WarnLegacyPathDslOnce()
        {
            if (s_WarnedLegacyPathDsl) return;
            s_WarnedLegacyPathDsl = true;

            Debug.LogWarning(
                "[KIBAMaterialGUI] Legacy displayName path DSL \"[Group] Label\" is deprecated and ignored. " +
                "Use ShaderLab attributes like [Group(My,Path)] instead. " +
                "GroupPath-based editor injections also require [Group(...)] to match.");
        }

        private static void WarnLegacyHintDslOnce()
        {
            if (s_WarnedLegacyHintDsl) return;
            s_WarnedLegacyHintDsl = true;

            Debug.LogWarning(
                "[KIBAMaterialGUI] Legacy displayName hint DSL \"{gradient}\" / \"{flexible}\" is deprecated. " +
                "Use ShaderLab attributes like [GradientTexture] and [FlexibleRange] instead.");
        }
    }
}


