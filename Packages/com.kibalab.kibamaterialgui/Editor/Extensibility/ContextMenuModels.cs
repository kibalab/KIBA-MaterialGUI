#nullable enable

using System;
using System.Collections.Generic;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    public enum ContextMenuItemType
    {
        Toggle,
        Button,
        Separator
    }

    public sealed class ContextMenuItem
    {
        public string Id = string.Empty;
        public ContextMenuItemType Type;
        public string Label = string.Empty;
        public bool Visible = true;
        public bool Enabled = true;
        public bool Locked;
        public Func<bool>? CheckedGetter;
        public Action? OnClickOrToggle;
        public int Order = 0;
    }

    public sealed class ContextMenuModel
    {
        public readonly List<ContextMenuItem> Items = new();

        public ContextMenuItem Add(ContextMenuItem i)
        {
            if (i == null) throw new ArgumentNullException(nameof(i));
            Items.Add(i);
            return i;
        }

        public bool Remove(string id) => Items.RemoveAll(x => x.Id == id) > 0;
        public ContextMenuItem? Find(string id) => Items.Find(x => x.Id == id);

        public void Hide(string id)
        {
            var i = Find(id);
            if (i != null) i.Visible = false;
        }

        public void Lock(string id, bool locked = true)
        {
            var i = Find(id);
            if (i != null)
            {
                i.Locked = locked;
                i.Enabled = !locked;
            }
        }
    }
}


