#nullable enable

using System;
using UnityEditor;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class MaterialGUIPropertyRendererAttribute : Attribute
    {
        public int Order { get; set; }
        public string[] ShaderNameEquals { get; set; } = Array.Empty<string>();
        public string[] ShaderNameContains { get; set; } = Array.Empty<string>();
        public string ShaderNameRegex { get; set; } = string.Empty;
        public string[] RequireProperties { get; set; } = Array.Empty<string>();
        public string[] RequireKeywords { get; set; } = Array.Empty<string>();
        public string? PropertyName { get; set; }
        public MaterialProperty.PropType[] PropertyTypes { get; set; } = Array.Empty<MaterialProperty.PropType>();
        public string[] RequireShaderAttributes { get; set; } = Array.Empty<string>();
        public string[] ExcludeShaderAttributes { get; set; } = Array.Empty<string>();
    }
}


