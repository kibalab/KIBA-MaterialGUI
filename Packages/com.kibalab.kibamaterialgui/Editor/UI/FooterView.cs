#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI
{
    internal class FooterView
    {
        private bool _diagnosticsExpanded;

        public void Draw(EditorContext ctx)
        {
            DrawDiagnostics(ctx);

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label($"Shader: {ctx.Material.shader.name}", EditorStyles.miniLabel, GUILayout.MinWidth(120));
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Properties: {ctx.Properties.Count}", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(2);
        }

        private void DrawDiagnostics(EditorContext ctx)
        {
            if (ctx?.Model == null) return;

            var diagnostics = ctx.Model.Diagnostics;

            var warningCount = 0;
            var errorCount = 0;
            for (var i = 0; i < diagnostics.Count; i++)
            {
                if (diagnostics[i].Severity == MaterialGUIDiagnosticSeverity.Error) errorCount++;
                else if (diagnostics[i].Severity == MaterialGUIDiagnosticSeverity.Warning) warningCount++;
            }

            if (diagnostics.Count == 0) return;

            EditorGUILayout.Space(4);
            _diagnosticsExpanded = EditorGUILayout.Foldout(
                _diagnosticsExpanded,
                $"Shader GUI Diagnostics: {errorCount} errors, {warningCount} warnings, {diagnostics.Count - errorCount - warningCount} info",
                true);

            if (!_diagnosticsExpanded) return;

            for (var i = 0; i < diagnostics.Count; i++)
            {
                var d = diagnostics[i];
                var type = d.Severity == MaterialGUIDiagnosticSeverity.Error
                    ? MessageType.Error
                    : d.Severity == MaterialGUIDiagnosticSeverity.Warning ? MessageType.Warning : MessageType.Info;
                var prefix = string.IsNullOrEmpty(d.PropertyName) ? string.Empty : $"{d.PropertyName}: ";
                EditorGUILayout.HelpBox(prefix + d.Message, type);
            }
        }
    }
}


