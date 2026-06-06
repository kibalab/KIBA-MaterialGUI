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

            var diagnostics = new System.Collections.Generic.List<MaterialGUIDiagnostic>(ctx.Model.Diagnostics);
            ContributionRegistry.ApplyDiagnostics(ctx, diagnostics);

            var warningCount = 0;
            var errorCount = 0;
            for (var i = 0; i < diagnostics.Count; i++)
            {
                if (diagnostics[i].Severity == MaterialGUIDiagnosticSeverity.Error) errorCount++;
                else if (diagnostics[i].Severity == MaterialGUIDiagnosticSeverity.Warning) warningCount++;
            }

            if (warningCount == 0 && errorCount == 0) return;

            EditorGUILayout.Space(4);
            _diagnosticsExpanded = EditorGUILayout.Foldout(
                _diagnosticsExpanded,
                $"Shader GUI Diagnostics: {errorCount} errors, {warningCount} warnings",
                true);

            if (!_diagnosticsExpanded) return;

            for (var i = 0; i < diagnostics.Count; i++)
            {
                var d = diagnostics[i];
                if (d.Severity == MaterialGUIDiagnosticSeverity.Info) continue;

                var type = d.Severity == MaterialGUIDiagnosticSeverity.Error
                    ? MessageType.Error
                    : MessageType.Warning;
                var prefix = string.IsNullOrEmpty(d.PropertyName) ? string.Empty : $"{d.PropertyName}: ";
                EditorGUILayout.HelpBox(prefix + d.Message, type);
            }
        }
    }
}


