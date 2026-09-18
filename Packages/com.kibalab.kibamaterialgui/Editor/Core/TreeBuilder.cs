using System;
using System.Collections.Generic;
using UnityEditor;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    internal static class TreeBuilder
    {
        internal static TreeNode Build(EditorContext ctx)
        {
            var model = MaterialGUIModelBuilder.Build(ctx);
            ctx.Model = model;
            return Convert(model.Root);
        }

        internal static TreeNode FindByPath(TreeNode root, string pathKey)
        {
            if (root == null) return null;
            if (string.IsNullOrEmpty(pathKey) || pathKey == "ROOT") return root;

            var parts = pathKey.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var cur = root;
            foreach (var part in parts)
            {
                if (!cur.Children.TryGetValue(part, out var nxt)) return null;
                cur = nxt;
            }

            return cur;
        }

        internal static TreeNode Convert(GroupNodeModel model)
        {
            var node = new TreeNode
            {
                Name = model.Name,
                PathKey = model.PathKey,
                Expanded = model.Expanded,
                Model = model
            };

            for (var i = 0; i < model.Properties.Count; i++)
            {
                var propertyModel = model.Properties[i];
                node.Properties.Add(new TreeNodeProperty
                {
                    Model = propertyModel
                });
            }

            foreach (var child in model.Children)
                node.Children.Add(child.Key, Convert(child.Value));

            return node;
        }
    }

    internal sealed class TreeNodeProperty
    {
        public MaterialProperty Property => Model.Property;
        public string Label => Model.Label;
        public ShaderPropertyModel Model;
    }

    internal sealed class TreeNode
    {
        public string Name;
        public string PathKey;
        public List<TreeNodeProperty> Properties { get; } = new();
        public Dictionary<string, TreeNode> Children { get; } = new();
        private bool _expanded = true;
        public bool Expanded
        {
            get => Model != null ? Model.Expanded : _expanded;
            set { _expanded = value; if (Model != null) Model.Expanded = value; }
        }
        public GroupNodeModel Model;
    }
}


