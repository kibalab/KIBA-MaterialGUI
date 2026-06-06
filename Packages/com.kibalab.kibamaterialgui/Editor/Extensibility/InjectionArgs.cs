#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    public readonly struct InjectionArgs
    {
        public readonly HookPoint Hook;
        public readonly MaterialGUIContext Context;
        public readonly string? GroupPath;
        public readonly MaterialProperty? Property;
        public readonly string? PropertyLabel;
        public readonly int Depth;
        internal readonly EditorContext? InternalContext;

        public InjectionArgs(
            HookPoint hook,
            MaterialGUIContext context,
            string? groupPath = null,
            MaterialProperty? property = null,
            string? propertyLabel = null,
            int depth = 0)
        {
            Hook = hook;
            Context = context;
            InternalContext = context as EditorContext;
            GroupPath = groupPath;
            Property = property;
            PropertyLabel = propertyLabel;
            Depth = depth;
        }
    }
}


