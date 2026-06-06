#nullable enable

using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    internal class RenderQueueController
    {
        public const int MixedRenderQueue = int.MinValue;

        public int GetCurrentRenderQueueValue(EditorContext ctx)
        {
            var targets = GetTargetMaterials(ctx);
            if (targets.Length == 0) return -1;

            var first = GetEffectiveRenderQueue(targets[0]);
            for (int i = 1; i < targets.Length; i++)
            {
                if (GetEffectiveRenderQueue(targets[i]) != first)
                    return MixedRenderQueue;
            }

            return first;
        }

        public string GetRenderQueueLabel(EditorContext ctx)
        {
            var fromShader = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_FromShader", "From Shader") ?? "From Shader";
            var background = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Background", "Background") ?? "Background";
            var geometry = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Geometry", "Geometry") ?? "Geometry";
            var alphatest = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_AlphaTest", "AlphaTest") ?? "AlphaTest";
            var transparent = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Transparent", "Transparent") ?? "Transparent";
            var overlay = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Overlay", "Overlay") ?? "Overlay";
            var mixed = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:Mixed", "Mixed") ?? "Mixed";

            var targets = GetTargetMaterials(ctx);
            if (targets.Length == 0) return fromShader;

            var allFromShader = true;
            var allExplicit = true;
            for (var i = 0; i < targets.Length; i++)
            {
                var mat = targets[i];
                if (mat == null) continue;
                if (mat.renderQueue < 0)
                {
                    allExplicit = false;
                }
                else
                {
                    allFromShader = false;
                }
            }

            var current = GetCurrentRenderQueueValue(ctx);
            if (current == MixedRenderQueue || (!allFromShader && !allExplicit))
                return mixed;

            if (allFromShader)
                return $"{fromShader} ({current})";

            return current switch
            {
                1000 => $"{background} (1000)",
                2000 => $"{geometry} (2000)",
                2450 => $"{alphatest} (2450)",
                3000 => $"{transparent} (3000)",
                4000 => $"{overlay} (4000)",
                _ => $"Custom ({current})"
            };
        }

        public void ShowRenderQueueMenu(EditorContext ctx, Rect anchor)
        {
            var targets = GetTargetMaterials(ctx);
            if (targets.Length == 0) return;

            var fromShader = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_FromShader", "From Shader") ?? "From Shader";
            var background = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Background", "Background") ?? "Background";
            var geometry = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Geometry", "Geometry") ?? "Geometry";
            var alphatest = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_AlphaTest", "AlphaTest") ?? "AlphaTest";
            var transparent = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Transparent", "Transparent") ?? "Transparent";
            var overlay = ctx.LocalizationStore?.Get(ctx.CurrentLanguage, "ui:RQ_Overlay", "Overlay") ?? "Overlay";

            var menu = new GenericMenu();

            var allFromShader = true;
            for (var i = 0; i < targets.Length; i++)
            {
                var mat = targets[i];
                if (mat == null || mat.renderQueue < 0) continue;
                allFromShader = false;
                break;
            }

            menu.AddItem(
                    new GUIContent(fromShader),
                    allFromShader,
                    () => SetRenderQueue(targets, -1));

            menu.AddSeparator("");

            Add(background, 1000);
            Add(geometry, 2000);
            Add(alphatest, 2450);
            Add(transparent, 3000);
            Add(overlay, 4000);

            menu.DropDown(anchor);
            return;

            void Add(string label, int value)
            {
                var allValue = true;
                for (var i = 0; i < targets.Length; i++)
                {
                    var mat = targets[i];
                    if (mat == null || mat.renderQueue == value) continue;
                    allValue = false;
                    break;
                }

                menu.AddItem(
                    new GUIContent($"{label}  ({value})"),
                    allValue,
                    () => SetRenderQueue(targets, value));
            }
        }

        public void SetRenderQueueValue(EditorContext ctx, int value)
        {
            var targets = GetTargetMaterials(ctx);
            SetRenderQueue(targets, value);
        }

        private static void SetRenderQueue(Material[] targets, int value)
        {
            if (targets == null || targets.Length == 0) return;

            var count = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                    count++;
            }

            if (count == 0) return;
            var objects = new Object[count];
            var index = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                var mat = targets[i];
                if (mat == null) continue;
                objects[index++] = mat;
            }

            Undo.RecordObjects(objects, "Set Render Queue");
            for (int i = 0; i < targets.Length; i++)
            {
                var m = targets[i];
                if (m == null) continue;
                m.renderQueue = value;
                EditorUtility.SetDirty(m);
            }
        }

        private static int GetEffectiveRenderQueue(Material m)
        {
            if (m == null) return -1;
            if (m.renderQueue >= 0) return m.renderQueue;
            if (m.shader != null) return m.shader.renderQueue;
            return -1;
        }

        private static Material[] GetTargetMaterials(EditorContext ctx)
        {
            if (ctx?.Targets != null && ctx.Targets.Count > 0)
            {
                var targets = new Material[ctx.Targets.Count];
                for (var i = 0; i < ctx.Targets.Count; i++)
                    targets[i] = ctx.Targets[i];
                return targets;
            }

            if (ctx?.Material != null)
                return new[] { ctx.Material };
            return System.Array.Empty<Material>();
        }
    }
}


