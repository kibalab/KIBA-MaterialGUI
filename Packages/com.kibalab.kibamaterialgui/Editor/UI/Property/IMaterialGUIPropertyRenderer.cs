#nullable enable

using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    public interface IMaterialGUIPropertyRenderer
    {
        float GetHeight(PropertyRendererArgs args);
        Rect OnGUI(PropertyRendererArgs args);
    }

    public interface IMaterialGUIPropertyRendererFilter
    {
        bool CanRender(PropertyRendererArgs args);
    }
}


