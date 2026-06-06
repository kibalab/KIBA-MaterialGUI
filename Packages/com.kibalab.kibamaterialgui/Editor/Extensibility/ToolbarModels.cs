#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    public enum ToolbarItemType
    {
        Toggle,
        Button,
        Input,
        Dropdown
    }

    public enum InputKind
    {
        Text,
        Int,
        Float
    }

    public sealed class DropdownOption
    {
        public string Id = string.Empty;
        public string Label = string.Empty;
        public Func<bool>? CheckedGetter;
        public Action? OnSelect;
        public bool Enabled = true;
        public int Order = 0;
        public string Tooltip = string.Empty;
    }

    public sealed class ToolbarItem
    {
        public string Id = string.Empty;
        public ToolbarItemType Type;
        public string Label = string.Empty;
        public float Width = 100f;
        public bool Visible = true;
        public bool Enabled = true;
        public bool Locked;
        public string Tooltip = string.Empty;

        public Func<bool>? ToggleGetter;
        public Action<bool>? ToggleSetter;

        public InputKind InputKind = InputKind.Text;
        public Func<string>? TextGetter;
        public Action<string>? TextSetter;
        public Func<int>? IntGetter;
        public Action<int>? IntSetter;
        public Func<float>? FloatGetter;
        public Action<float>? FloatSetter;
        public string Placeholder = string.Empty;

        public Func<string>? DropdownLabelGetter;
        public List<DropdownOption> Options = new();

        public Action? OnClick;
        public Action<Rect>? CustomDrawer;

        public int Order = 0;
    }

    public sealed class ToolbarModel
    {
        public readonly List<ToolbarItem> Items = new();

        public ToolbarItem Add(ToolbarItem i)
        {
            if (i == null) throw new ArgumentNullException(nameof(i));
            Items.Add(i);
            return i;
        }

        public bool Remove(string id) => Items.RemoveAll(x => x.Id == id) > 0;
        public ToolbarItem? Find(string id) => Items.Find(x => x.Id == id);

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


