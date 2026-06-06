#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.Core;

namespace KIBA_.KIBAMaterialGUI.Editor.Localization
{
    internal static class LocalizationHelper
    {
        public static string TranslateGroup(EditorContext ctx, string groupRaw, string pathKey)
        {
            if (ctx.LocalizationStore == null) return groupRaw;

            var full = ShaderLocalizationUtil.SanitizeKey($"group:{pathKey}");
            var byFull = ctx.LocalizationStore.Get(ctx.CurrentLanguage, full, pathKey);
            if (!string.IsNullOrEmpty(byFull)) return byFull;

            var nameKey = ShaderLocalizationUtil.SanitizeKey($"groupName:{groupRaw}");
            var byName = ctx.LocalizationStore.Get(ctx.CurrentLanguage, nameKey, groupRaw);
            return byName ?? groupRaw;
        }

        public static string TranslateProp(EditorContext ctx, string propName, string labelRaw)
        {
            if (ctx.LocalizationStore == null) return labelRaw;

            var nameKey = ShaderLocalizationUtil.SanitizeKey($"propName:{propName}");
            var byName = ctx.LocalizationStore.Get(ctx.CurrentLanguage, nameKey, propName);
            if (!string.IsNullOrEmpty(byName)) return byName;

            var labelKey = ShaderLocalizationUtil.SanitizeKey($"prop:{labelRaw}");
            return ctx.LocalizationStore.Get(ctx.CurrentLanguage, labelKey, labelRaw);
        }
    }
}


