#nullable enable

using System.Text;

namespace KIBA_.KIBAMaterialGUI.Editor.Localization
{
    static class ShaderLocalizationUtil
    {
        public static string SanitizeKey(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            s = s.Normalize(NormalizationForm.FormC).Replace('／', '/').Replace('\u00A0', ' ').Replace('\u3000', ' ').Trim();
            return s;
        }
    }
}

