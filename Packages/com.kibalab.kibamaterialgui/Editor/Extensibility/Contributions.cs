#nullable enable

using System;
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    public enum ContributionTarget
    {
        Toolbar,
        GroupContextMenu,
        GroupAction,
        Diagnostic,
        Filter
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class MaterialGUIContributionAttribute : Attribute
    {
        public ContributionTarget Target { get; }
        public int Order { get; set; }
        public string[] ShaderNameEquals { get; set; } = Array.Empty<string>();
        public string[] ShaderNameContains { get; set; } = Array.Empty<string>();
        public string ShaderNameRegex { get; set; } = string.Empty;
        public string GroupPath { get; set; } = string.Empty;

        public MaterialGUIContributionAttribute(ContributionTarget target)
        {
            Target = target;
        }
    }

    public interface IToolbarContributor
    {
        void Contribute(ToolbarModel model, InjectionArgs args);
    }

    public interface IGroupMenuContributor
    {
        void Contribute(ContextMenuModel model, InjectionArgs args);
    }

    public interface IMaterialGUIGroupActionContributor
    {
        void Contribute(GroupActionModel model, InjectionArgs args);
    }

    public interface IMaterialGUIDiagnosticProvider
    {
        void Contribute(List<MaterialGUIDiagnostic> diagnostics, InjectionArgs args);
    }

    public interface IMaterialGUIFilterProvider
    {
        void Contribute(ToolbarModel model, InjectionArgs args);
    }
}


