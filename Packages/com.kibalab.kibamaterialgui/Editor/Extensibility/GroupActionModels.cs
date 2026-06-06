#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    public sealed class GroupActionItem
    {
        public string Id = string.Empty;
        public GUIContent Content = GUIContent.none;
        public string Tooltip = string.Empty;
        public bool Visible = true;
        public bool Enabled = true;
        public int Order;
        public Action<Rect>? OnGUI;
        public Action? OnClick;
    }

    public sealed class GroupActionModel
    {
        public readonly List<GroupActionItem> Items = new();
        public Action? BeginDrag;

        public GroupActionItem Add(GroupActionItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Items.Add(item);
            return item;
        }

        public bool Remove(string id) => Items.RemoveAll(x => x.Id == id) > 0;
        public GroupActionItem? Find(string id) => Items.Find(x => x.Id == id);
    }
}


