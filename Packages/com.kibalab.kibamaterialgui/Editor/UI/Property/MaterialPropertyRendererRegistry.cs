#nullable enable

using System;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    internal sealed class MaterialPropertyRendererRegistry
    {
        public readonly struct ResolvedPropertyRenderer
        {
            private readonly IMaterialGUIPropertyRenderer? _extension;
            private readonly IMaterialPropertyRenderer? _materialRenderer;

            internal ResolvedPropertyRenderer(IMaterialGUIPropertyRenderer extension)
            {
                _extension = extension;
                _materialRenderer = null;
            }

            internal ResolvedPropertyRenderer(IMaterialPropertyRenderer materialRenderer)
            {
                _extension = null;
                _materialRenderer = materialRenderer;
            }

            public float GetHeight(in PropertyRendererArgs args)
            {
                if (_extension != null)
                    return _extension.GetHeight(args);
                return _materialRenderer != null
                    ? _materialRenderer.GetHeight(args)
                    : EditorGUIUtility.singleLineHeight;
            }

            public Rect OnGUI(in PropertyRendererArgs args)
            {
                if (_extension != null)
                    return _extension.OnGUI(args);
                return _materialRenderer != null
                    ? _materialRenderer.OnGUI(args)
                    : args.Position;
            }
        }

        private readonly List<IMaterialPropertyRenderer> _builtInRenderers = new();

        public MaterialPropertyRendererRegistry()
        {
            _builtInRenderers.Add(new GradientTexturePropertyRenderer());
            _builtInRenderers.Add(new SegmentedEnumPropertyRenderer());
            _builtInRenderers.Add(new KeywordEnumPropertyRenderer());
            _builtInRenderers.Add(new EnumPropertyRenderer());
            _builtInRenderers.Add(new TogglePropertyRenderer());
            _builtInRenderers.Add(new FlexibleRangePropertyRenderer());
            _builtInRenderers.Add(new RangePropertyRenderer());
            _builtInRenderers.Add(new FloatPropertyRenderer());
            _builtInRenderers.Add(new ColorPropertyRenderer());
            _builtInRenderers.Add(new MinMaxSliderPropertyRenderer());
            _builtInRenderers.Add(new VectorPropertyRenderer());
            _builtInRenderers.Add(new TexturePropertyRenderer());
            _builtInRenderers.Add(new DefaultMaterialPropertyRenderer());
        }

        private IMaterialPropertyRenderer GetBuiltInRenderer(in PropertyRendererArgs args)
        {
            for (int i = 0; i < _builtInRenderers.Count; i++)
            {
                var r = _builtInRenderers[i];
                if (!r.CanRender(args)) continue;
                return r;
            }

            return _builtInRenderers[^1];
        }

        public float GetHeight(MaterialProperty property, string label)
        {
            return GetHeight(null, ResolveMaterial(property), property, label, EditorStyles.centeredGreyMiniLabel);
        }

        public float GetHeight(
            MaterialEditor? editor,
            Material? material,
            MaterialProperty property,
            string label,
            GUIStyle miniGray)
        {
            var args = new PropertyRendererArgs(Rect.zero, editor, material, property, label, miniGray);
            var resolved = Resolve(args);
            return resolved.GetHeight(args);
        }

        public Rect Render(Rect rect, MaterialEditor editor, Material material, MaterialProperty property, string label, GUIStyle miniGray)
        {
            var args = new PropertyRendererArgs(rect, editor, material, property, label, miniGray);
            return Render(args);
        }

        public Rect Render(
            Rect rect,
            MaterialEditor editor,
            Material material,
            MaterialProperty property,
            string label,
            GUIStyle miniGray,
            in PropertyRowLayout layout)
        {
            var args = new PropertyRendererArgs(rect, editor, material, property, label, miniGray, layout);
            return Render(args);
        }

        public Rect Render(in PropertyRendererArgs args)
        {
            var resolved = Resolve(args);
            return resolved.OnGUI(args);
        }

        public ResolvedPropertyRenderer Resolve(in PropertyRendererArgs args)
        {
            if (MaterialGUIPropertyRendererRegistry.TryFindRenderer(args, out var extension) && extension != null)
                return new ResolvedPropertyRenderer(extension);

            return new ResolvedPropertyRenderer(GetBuiltInRenderer(args));
        }

        private static Material? ResolveMaterial(MaterialProperty property)
        {
            var targets = property.targets;
            if (targets == null || targets.Length == 0) return null;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is Material m) return m;
            }

            return null;
        }
    }
}


