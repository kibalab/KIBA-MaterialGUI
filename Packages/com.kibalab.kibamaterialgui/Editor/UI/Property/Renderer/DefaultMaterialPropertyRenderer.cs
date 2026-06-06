#nullable enable

using System.Reflection;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property.Renderer
{
    internal sealed class DefaultMaterialPropertyRenderer : IMaterialPropertyRenderer
    {
        private static readonly MethodInfo? GetDefaultHeightMethod = typeof(MaterialEditor).GetMethod(
            "GetDefaultPropertyHeight",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(MaterialProperty) },
            null);

        private static readonly MethodInfo? DefaultShaderPropertyRectMethod = typeof(MaterialEditor).GetMethod(
            "DefaultShaderProperty",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(Rect), typeof(MaterialProperty), typeof(string) },
            null);

        private static readonly MethodInfo? ShaderPropertyRectMethod = typeof(MaterialEditor).GetMethod(
            "ShaderProperty",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(Rect), typeof(MaterialProperty), typeof(string) },
            null);

        public bool CanRender(in PropertyRendererArgs args)
        {
            return true;
        }

        public float GetHeight(in PropertyRendererArgs args)
        {
            if (GetDefaultHeightMethod != null)
            {
                try
                {
                    if (GetDefaultHeightMethod.Invoke(null, new object[] { args.Property }) is float h)
                        return h;
                }
                catch (System.Exception ex)
                {
                    MaterialGUIInternalDiagnostics.WarnOnce(
                        "default-renderer.height:" + ex.GetType().FullName,
                        "Failed to query Unity default material property height: " + ex.Message);
                }
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public Rect OnGUI(in PropertyRendererArgs args)
        {
            if (args.MaterialEditor == null) return args.Position;
            var rect = args.Layout.IsValid ? args.Layout.MainRect : args.Position;
            var label = args.Label;
            var editor = args.MaterialEditor;

            if (DefaultShaderPropertyRectMethod != null)
            {
                try
                {
                    DefaultShaderPropertyRectMethod.Invoke(editor, new object[] { rect, args.Property, label });
                    return rect;
                }
                catch (System.Exception ex)
                {
                    MaterialGUIInternalDiagnostics.WarnOnce(
                        "default-renderer.default-property:" + ex.GetType().FullName,
                        "Failed to draw Unity default shader property: " + ex.Message);
                }
            }

            if (ShaderPropertyRectMethod != null)
            {
                try
                {
                    ShaderPropertyRectMethod.Invoke(editor, new object[] { rect, args.Property, label });
                    return rect;
                }
                catch (System.Exception ex)
                {
                    MaterialGUIInternalDiagnostics.WarnOnce(
                        "default-renderer.shader-property:" + ex.GetType().FullName,
                        "Failed to draw Unity shader property: " + ex.Message);
                }
            }

            editor.ShaderProperty(args.Property, label);
            return GUILayoutUtility.GetLastRect();
        }
    }
}


