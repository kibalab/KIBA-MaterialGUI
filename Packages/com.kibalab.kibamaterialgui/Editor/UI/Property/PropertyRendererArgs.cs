#nullable enable

using System;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    public readonly struct PropertyRendererArgs
    {
        public readonly Rect Position;
        public readonly PropertyRowLayout Layout;
        public readonly MaterialEditor? MaterialEditor;
        public readonly Material? Material;
        public readonly MaterialProperty Property;
        public readonly string Label;
        public readonly GUIStyle MiniGray;
        public readonly Shader? Shader;
        public readonly ShaderPropertyModel? Model;

        public Rect LabelRect => Layout.LabelRect;
        public Rect FieldRect => Layout.FieldRect;
        public Rect ResetRect => Layout.ResetRect;

        public PropertyRendererArgs(
            Rect position,
            MaterialEditor? materialEditor,
            Material? material,
            MaterialProperty property,
            string label,
            GUIStyle miniGray,
            PropertyRowLayout layout = default,
            ShaderPropertyModel? model = null)
        {
            Position = position;
            Layout = layout;
            MaterialEditor = materialEditor;
            Material = material;
            Property = property;
            Label = label ?? string.Empty;
            MiniGray = miniGray ?? EditorStyles.centeredGreyMiniLabel;
            Shader = ResolveShader(material, property);
            Model = model;
        }

        public bool HasShaderAttribute(string attributeName)
        {
            return ShaderPropertyAttributeCache.HasShaderAttribute(Shader, Property.name, attributeName);
        }

        public bool TryGetShaderAttribute(string attributeName, out ShaderPropertyAttributeCache.ShaderAttributeInfo attribute)
        {
            return ShaderPropertyAttributeCache.TryGetShaderAttribute(Shader, Property.name, attributeName, out attribute);
        }

        public IReadOnlyList<ShaderPropertyAttributeCache.ShaderAttributeInfo> GetShaderAttributes()
        {
            return ShaderPropertyAttributeCache.TryGetShaderAttributes(Shader, Property.name, out var attrs)
                ? attrs
                : Array.Empty<ShaderPropertyAttributeCache.ShaderAttributeInfo>();
        }

        public void SetFloatValue(float value, string? undoName = null)
        {
            if (Mathf.Approximately(Property.floatValue, value)) return;
            RegisterPropertyValueChange(undoName);
            Property.floatValue = value;
        }

        public void SetColorValue(Color value, string? undoName = null)
        {
            if (Property.colorValue == value) return;
            RegisterPropertyValueChange(undoName);
            Property.colorValue = value;
        }

        public void SetVectorValue(Vector4 value, string? undoName = null)
        {
            if (Property.vectorValue == value) return;
            RegisterPropertyValueChange(undoName);
            Property.vectorValue = value;
        }

        public void SetTextureValue(Texture? value, string? undoName = null)
        {
            if (Property.textureValue == value) return;
            RegisterPropertyValueChange(undoName);
            Property.textureValue = value;
        }

        public void SetTextureScaleAndOffset(Vector4 value, string? undoName = null)
        {
            if (Property.textureScaleAndOffset == value) return;
            RegisterPropertyValueChange(undoName);
            Property.textureScaleAndOffset = value;
        }

        public void RegisterPropertyValueChange(string? undoName = null)
        {
            PropertyRowHost.RegisterPropertyValueChange(
                MaterialEditor,
                Property,
                string.IsNullOrWhiteSpace(undoName) ? $"Change {Label}" : undoName!);
        }

        private static Shader? ResolveShader(Material? material, MaterialProperty property)
        {
            if (material != null && material.shader != null) return material.shader;

            var targets = property.targets;
            if (targets == null || targets.Length == 0) return null;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is Material m && m.shader != null)
                    return m.shader;
            }

            return null;
        }
    }
}


