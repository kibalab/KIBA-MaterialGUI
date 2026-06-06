#nullable enable

using System.Reflection;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.ShaderLab
{
    public abstract class ShaderPropertyDrawer : MaterialPropertyDrawer
    {
        private static readonly MethodInfo? GetDefaultHeightMethod = typeof(MaterialEditor).GetMethod(
            "GetDefaultPropertyHeight",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(MaterialProperty) },
            null);

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (GetDefaultHeightMethod != null)
            {
                try
                {
                    if (GetDefaultHeightMethod.Invoke(null, new object[] { prop }) is float h) return h;
                }
                catch (System.Exception ex)
                {
                    MaterialGUIInternalDiagnostics.WarnOnce(
                        "drawer.default-height:" + ex.GetType().FullName,
                        "Failed to query Unity default material property height: " + ex.Message);
                }
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            editor.DefaultShaderProperty(position, prop, label);
        }
    }

    public sealed class GroupDrawer : ShaderPropertyDrawer
    {
        public GroupDrawer()
        {
        }

        public GroupDrawer(string groupPath)
        {
        }
    }

    public sealed class GradientTextureDrawer : ShaderPropertyDrawer
    {
    }

    public sealed class FlexibleRangeDrawer : ShaderPropertyDrawer
    {
        public FlexibleRangeDrawer() { }
        public FlexibleRangeDrawer(float min, float max) { }
        public FlexibleRangeDrawer(string args) { }
        public FlexibleRangeDrawer(string min, float max) { }
        public FlexibleRangeDrawer(float min, string max) { }
        public FlexibleRangeDrawer(string min, string max) { }
    }

    public sealed class SpaceDrawer : ShaderPropertyDrawer
    {
        public SpaceDrawer()
        {
        }

        public SpaceDrawer(float px)
        {
        }

        public SpaceDrawer(string px)
        {
        }
    }

    public sealed class DividerDrawer : ShaderPropertyDrawer
    {
    }

    public sealed class SegmentedEnumDrawer : ShaderPropertyDrawer
    {
    }

    public sealed class SegmentedDrawer : ShaderPropertyDrawer
    {
    }

    public sealed class UnitDrawer : ShaderPropertyDrawer
    {
        public UnitDrawer() { }
        public UnitDrawer(string unit) { }
    }

    public sealed class MinMaxSliderDrawer : ShaderPropertyDrawer
    {
        public MinMaxSliderDrawer() { }
        public MinMaxSliderDrawer(float min, float max) { }
        public MinMaxSliderDrawer(string args) { }
    }

    public sealed class VectorDrawer : ShaderPropertyDrawer
    {
        public VectorDrawer()
        {
        }

        public VectorDrawer(float fieldCount)
        {
        }

        public VectorDrawer(string fieldCount)
        {
        }
    }

    public sealed class Vector2Drawer : ShaderPropertyDrawer
    {
    }

    public sealed class Vector3Drawer : ShaderPropertyDrawer
    {
    }

    public sealed class Vector4Drawer : ShaderPropertyDrawer
    {
    }

    public sealed class ValidateDrawer : ShaderPropertyDrawer
    {
        public ValidateDrawer()
        {
        }

        public ValidateDrawer(string methodPath)
        {
        }

        public ValidateDrawer(string methodPath, string assemblyName)
        {
        }
    }

    public sealed class ShowIfDrawer : ShaderPropertyDrawer
    {
        public ShowIfDrawer()
        {
        }

        public ShowIfDrawer(string propertyName)
        {
        }

        public ShowIfDrawer(string propertyName, float expectedValue)
        {
        }
    }
}


