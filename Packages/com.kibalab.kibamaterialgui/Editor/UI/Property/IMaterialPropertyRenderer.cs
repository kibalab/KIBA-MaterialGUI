#nullable enable

using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    internal interface IMaterialPropertyRenderer
    {
        bool CanRender(in PropertyRendererArgs args);
        float GetHeight(in PropertyRendererArgs args);
        Rect OnGUI(in PropertyRendererArgs args);
    }
}


