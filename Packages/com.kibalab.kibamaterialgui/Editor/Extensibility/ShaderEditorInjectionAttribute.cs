#nullable enable

using System;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ShaderEditorInjectionAttribute : Attribute
    {
        public HookPoint Hook { get; }
        public int Order { get; set; }
        public string[] ShaderNameEquals { get; set; } = Array.Empty<string>();
        public string[] ShaderNameContains { get; set; } = Array.Empty<string>();
        public string ShaderNameRegex { get; set; } = string.Empty;
        public string[] RequireProperties { get; set; } = Array.Empty<string>();
        public string[] RequireKeywords { get; set; } = Array.Empty<string>();
        public string? GroupPath { get; set; }
        public string? PropertyName { get; set; }

        public ShaderEditorInjectionAttribute(HookPoint hook)
        {
            Hook = hook;
        }
    }
}


