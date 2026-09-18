#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    public static class MaterialGUIPropertyChangeUtility
    {
        public static void RegisterChange(
            MaterialGUIContext? context,
            MaterialProperty property,
            string undoName = "Change Material Property")
        {
            if (property == null) return;

            PropertyRowHost.RegisterPropertyValueChange(context?.MaterialEditor, property, undoName);
        }

        public static void SetFloat(
            MaterialGUIContext? context,
            MaterialProperty property,
            float value,
            string undoName = "Change Material Float")
        {
            if (property == null || (!property.hasMixedValue && Mathf.Approximately(property.floatValue, value))) return;
            RegisterChange(context, property, undoName);
            property.floatValue = value;
        }

        public static void SetColor(
            MaterialGUIContext? context,
            MaterialProperty property,
            Color value,
            string undoName = "Change Material Color")
        {
            if (property == null || (!property.hasMixedValue && property.colorValue == value)) return;
            RegisterChange(context, property, undoName);
            property.colorValue = value;
        }

        public static void SetInt(
            MaterialGUIContext? context,
            MaterialProperty property,
            int value,
            string undoName = "Change Material Integer")
        {
            if (property == null || (!property.hasMixedValue && property.intValue == value)) return;
            RegisterChange(context, property, undoName);
            property.intValue = value;
        }

        public static void SetVector(
            MaterialGUIContext? context,
            MaterialProperty property,
            Vector4 value,
            string undoName = "Change Material Vector")
        {
            if (property == null || (!property.hasMixedValue && property.vectorValue == value)) return;
            RegisterChange(context, property, undoName);
            property.vectorValue = value;
        }

        public static void SetTexture(
            MaterialGUIContext? context,
            MaterialProperty property,
            Texture? value,
            string undoName = "Change Material Texture")
        {
            if (property == null || (!property.hasMixedValue && property.textureValue == value)) return;
            RegisterChange(context, property, undoName);
            property.textureValue = value;
        }

        public static void SetTextureScaleAndOffset(
            MaterialGUIContext? context,
            MaterialProperty property,
            Vector4 value,
            string undoName = "Change Material Texture Scale And Offset")
        {
            if (property == null || (!property.hasMixedValue && property.textureScaleAndOffset == value)) return;
            RegisterChange(context, property, undoName);
            property.textureScaleAndOffset = value;
        }
    }
}


