#nullable enable

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Core
{
    internal sealed class MaterialGUISession
    {
        private TreeNode? _root;
        private MaterialGUIModel? _model;
        private Shader? _shader;
        private int _signature;
        private int _projectVersion = -1;
        private string? _language;

        internal TreeNode GetTree(EditorContext ctx)
        {
            var shader = ctx.Material != null ? ctx.Material.shader : null;
            var signature = ComputeSignature(ctx.Properties);
            if (_root != null && _model != null && _shader == shader && _signature == signature &&
                _projectVersion == ShaderPropertyAttributeCache.Version && _language == ctx.CurrentLanguage)
            {
                ctx.Model = _model;
                MaterialGUIModelBuilder.Refresh(ctx, _model);
                return _root;
            }

            _root = TreeBuilder.Build(ctx);
            _model = ctx.Model;
            _shader = shader;
            _signature = signature;
            _projectVersion = ShaderPropertyAttributeCache.Version;
            _language = ctx.CurrentLanguage;
            return _root;
        }

        internal void Clear()
        {
            _root = null;
            _model = null;
            _shader = null;
        }

        private static int ComputeSignature(IReadOnlyList<MaterialProperty> properties)
        {
            unchecked
            {
                var hash = properties.Count;
                for (var i = 0; i < properties.Count; i++)
                {
                    var p = properties[i];
                    if (p == null) continue;
                    hash = hash * 31 + p.name.GetHashCode();
                    hash = hash * 31 + (p.displayName?.GetHashCode() ?? 0);
                    hash = hash * 31 + (int)p.type;
                    hash = hash * 31 + (int)p.flags;
                }
                return hash;
            }
        }
    }
}
