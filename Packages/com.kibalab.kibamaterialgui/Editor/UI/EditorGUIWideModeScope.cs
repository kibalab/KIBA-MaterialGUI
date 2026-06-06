using System;
using UnityEditor;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal readonly struct EditorGUIWideModeScope : IDisposable
    {
        private readonly bool _prevWide;
        private readonly float _prevLabelWidth;
        private readonly bool _setLabel;

        public EditorGUIWideModeScope(bool wide)
        {
            _prevWide = EditorGUIUtility.wideMode;
            _prevLabelWidth = EditorGUIUtility.labelWidth;
            _setLabel = false;
            EditorGUIUtility.wideMode = wide;
        }

        public EditorGUIWideModeScope(bool wide, float labelWidth)
        {
            _prevWide = EditorGUIUtility.wideMode;
            _prevLabelWidth = EditorGUIUtility.labelWidth;
            _setLabel = true;
            EditorGUIUtility.wideMode = wide;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public void Dispose()
        {
            EditorGUIUtility.wideMode = _prevWide;
            if (_setLabel) EditorGUIUtility.labelWidth = _prevLabelWidth;
        }
    }
}



