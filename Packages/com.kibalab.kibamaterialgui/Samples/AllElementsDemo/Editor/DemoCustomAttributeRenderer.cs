#nullable enable

using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Samples.Editor
{
    public sealed class DemoNoteDrawer : ShaderPropertyDrawer
    {
        public DemoNoteDrawer()
        {
        }

        public DemoNoteDrawer(string note)
        {
        }
    }

    [MaterialGUIPropertyRenderer(
        Order = -500,
        PropertyTypes = new[] { MaterialProperty.PropType.Float },
        RequireShaderAttributes = new[] { "DemoNote" }
    )]
    public sealed class DemoNoteFloatRenderer : ShaderPropertyRenderer<string>
    {
        protected override string AttributeName => "DemoNote";

        protected override bool TryParseArguments(
            in ShaderPropertyAttributeCache.ShaderAttributeInfo attribute,
            out string arguments)
        {
            arguments = string.Empty;
            var tokens = ShaderAttributeArgumentParser.Split(attribute.Args);
            if (tokens.Length == 0) return true;
            arguments = ShaderAttributeArgumentParser.TrimQuotes(tokens[0]);
            return true;
        }

        protected override float GetHeight(in ShaderAttributeArgs<string> args)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override Rect OnGUI(in ShaderAttributeArgs<string> args)
        {
            var note = args.Arguments;
            var shownLabel = string.IsNullOrEmpty(note) ? args.Label : $"{args.Label} ({note})";

            EditorGUI.BeginChangeCheck();
            var next = EditorGUI.FloatField(args.Position, shownLabel, args.Property.floatValue);
            if (EditorGUI.EndChangeCheck())
                args.Base.SetFloatValue(next);

            return args.Position;
        }
    }
}


