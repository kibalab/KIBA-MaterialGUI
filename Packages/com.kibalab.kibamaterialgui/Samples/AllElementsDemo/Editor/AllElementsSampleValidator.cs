#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.UI.Property;

namespace KIBA_.KIBAMaterialGUI.Samples.Editor
{
    public static class AllElementsSampleValidator
    {
        public static bool NonNegativeOnly(MaterialGUIPropertyValidateContext ctx)
        {
            if (ctx.Property.type != UnityEditor.MaterialProperty.PropType.Float &&
                ctx.Property.type != UnityEditor.MaterialProperty.PropType.Range)
                return true;

            return ctx.Property.floatValue >= 0f;
        }
    }
}


