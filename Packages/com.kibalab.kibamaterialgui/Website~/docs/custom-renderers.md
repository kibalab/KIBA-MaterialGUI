# Custom Renderers

Custom renderers change how one material property row is drawn.

Use a custom renderer when a ShaderLab attribute is not expressive enough for a property field. Good examples are stepped numeric fields, compact texture pickers, preview fields, channel-mask editors, or project-specific controls.

Do not use a custom renderer for whole-inspector helper UI. For toolbar buttons, group controls, diagnostics, or menus, use [Editor Injection](./editor-injection.md).

## When To Use This

Use a custom renderer when:

- one property needs a custom field,
- the value still belongs to a normal `MaterialProperty`,
- the UI should appear exactly where that property row appears,
- reset, animation tint, mixed values, and validation should stay integrated with KIBAMaterialGUI rows.

Avoid a custom renderer when:

- you want to add a button above a group,
- you want a toolbar action,
- you want a context menu item,
- you want a diagnostic message,
- you need a large workflow that deserves its own editor window.

## Learning Path

1. [Your First Renderer](./custom-renderers/first-renderer.md)
2. [ShaderLab Attribute Bridge](./custom-renderers/shaderlab-bridge.md)
3. [Matching And Priority](./custom-renderers/matching-and-priority.md)
4. [Typed Attribute Renderers](./custom-renderers/typed-attribute-renderers.md)
5. [Value Writes And Context](./custom-renderers/value-writes-and-context.md)

## Minimal Shape

```csharp
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

[MaterialGUIPropertyRenderer(
    Order = -100,
    PropertyTypes = new[] { MaterialProperty.PropType.Float },
    RequireShaderAttributes = new[] { "MyHint" })]
public sealed class MyHintRenderer : IMaterialGUIPropertyRenderer
{
    public float GetHeight(PropertyRendererArgs args)
        => EditorGUIUtility.singleLineHeight;

    public Rect OnGUI(PropertyRendererArgs args)
    {
        EditorGUI.BeginChangeCheck();
        var next = EditorGUI.FloatField(args.Position, args.Label, args.Property.floatValue);
        if (EditorGUI.EndChangeCheck())
            args.SetFloatValue(next);

        return args.Position;
    }
}
```

The important rule is simple: draw the field, and write values through `PropertyRendererArgs`.
