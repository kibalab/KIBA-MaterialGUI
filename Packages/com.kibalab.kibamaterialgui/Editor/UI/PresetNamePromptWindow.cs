#nullable enable

using System;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal sealed class PresetNamePromptWindow : EditorWindow
    {
        private Action<string> _onOk = static delegate { };
        private string _titleText = "New Preset";
        private string _labelText = "Name";
        private string _okText = "Create";
        private string _cancelText = "Cancel";
        private string _nameText = "";

        private bool _focusArmed;

        public static void Open(string titleText, string labelText, string? defaultName, string okText, string cancelText, Action<string> onOk)
        {
            var w = CreateInstance<PresetNamePromptWindow>();
            w._titleText = string.IsNullOrEmpty(titleText) ? "New Preset" : titleText;
            w._labelText = string.IsNullOrEmpty(labelText) ? "Name" : labelText;
            w._okText = string.IsNullOrEmpty(okText) ? "Create" : okText;
            w._cancelText = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;
            w._nameText = defaultName ?? "";
            w._onOk = onOk;

            w.titleContent = new GUIContent(w._titleText);
            w.minSize = new Vector2(360, 90);
            w.maxSize = new Vector2(600, 120);

            var pos = new Rect(Screen.currentResolution.width * 0.5f - 200, Screen.currentResolution.height * 0.5f - 60, 400, 100);
            w.position = pos;

            w.ShowUtility();
            w._focusArmed = true;
        }

        private void OnGUI()
        {
            GUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(_labelText, GUILayout.Width(80));
                GUI.SetNextControlName("PresetNameField");
                _nameText = EditorGUILayout.TextField(_nameText);
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(_cancelText, GUILayout.Width(90)))
                {
                    Close();
                }

                if (GUILayout.Button(_okText, GUILayout.Width(90)))
                {
                    Submit();
                }
            }

            if (_focusArmed)
            {
                _focusArmed = false;
                EditorGUI.FocusTextInControl("PresetNameField");
            }

            var e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                {
                    Submit();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    Close();
                    e.Use();
                }
            }
        }

        private void Submit()
        {
            var nameContext = _nameText.Trim();
            _onOk(nameContext);
            Close();
        }
    }
}

