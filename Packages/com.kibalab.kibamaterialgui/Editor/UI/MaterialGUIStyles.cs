#nullable enable

using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal class MaterialGUIStyles
    {
        public GUIStyle MiniGray = null!;
        public GUIStyle HeaderButton = null!;
        public GUIStyle TransparentTextButton = null!;

        public void Ensure()
        {
            if (HeaderButton != null) return;

            MiniGray = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 9,
                clipping = TextClipping.Clip
            };
            HeaderButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 28,
                padding = new RectOffset(6, 6, 4, 4)
            };
            TransparentTextButton = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = EditorStyles.miniLabel.normal.textColor },
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                clipping = TextClipping.Clip
            };
        }
    }
}


