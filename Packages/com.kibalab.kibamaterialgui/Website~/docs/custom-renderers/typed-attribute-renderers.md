# Typed Attribute Renderers

`ShaderPropertyRenderer<TArgs>` is a helper base class for renderers driven by one ShaderLab attribute.

It handles:

- finding the attribute,
- parsing arguments,
- filtering with `CanRender`,
- forwarding typed arguments to your drawing code.

## One Argument

ShaderLab:

```shaderlab
[RangeStep(0.25)] _Amount ("Amount", Float) = 1
```

C#:

```csharp
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

public sealed class RangeStepDrawer : KIBA_.KIBAMaterialGUI.Editor.ShaderLab.ShaderPropertyDrawer
{
    public RangeStepDrawer(float step) {}
}

[MaterialGUIPropertyRenderer(
    Order = -300,
    PropertyTypes = new[] { MaterialProperty.PropType.Float, MaterialProperty.PropType.Range },
    RequireShaderAttributes = new[] { "RangeStep" })]
public sealed class RangeStepRenderer : ShaderPropertyRenderer<float>
{
    protected override string AttributeName => "RangeStep";

    protected override float GetHeight(in ShaderAttributeArgs<float> args)
        => EditorGUIUtility.singleLineHeight;

    protected override Rect OnGUI(in ShaderAttributeArgs<float> args)
    {
        var step = Mathf.Max(0.0001f, args.Arguments);

        EditorGUI.BeginChangeCheck();
        var next = EditorGUI.FloatField(args.Position, args.Label, args.Property.floatValue);
        if (EditorGUI.EndChangeCheck())
            args.Base.SetFloatValue(Mathf.Round(next / step) * step);

        return args.Position;
    }
}
```

## Two Arguments

Use `ShaderPropertyRenderer<T1, T2>`.

```shaderlab
[ClampFloat(0, 10)] _Amount ("Amount", Float) = 1
```

```csharp
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

[MaterialGUIPropertyRenderer(
    Order = -250,
    PropertyTypes = new[] { MaterialProperty.PropType.Float },
    RequireShaderAttributes = new[] { "ClampFloat" })]
public sealed class ClampFloatRenderer : ShaderPropertyRenderer<float, float>
{
    protected override string AttributeName => "ClampFloat";

    protected override float GetHeight(PropertyRendererArgs args, float min, float max)
        => EditorGUIUtility.singleLineHeight;

    protected override Rect OnGUI(PropertyRendererArgs args, float min, float max)
    {
        EditorGUI.BeginChangeCheck();
        var next = EditorGUI.FloatField(args.Position, args.Label, args.Property.floatValue);
        if (EditorGUI.EndChangeCheck())
            args.SetFloatValue(Mathf.Clamp(next, min, max));

        return args.Position;
    }
}
```

## Supported Argument Types

The default binder supports common scalar values:

- `string`
- `bool`
- `int`
- `float`
- `double`
- `long`
- enum values

For complex parsing, override `TryParseArguments`.

```csharp
protected override bool TryParseArguments(
    in ShaderPropertyAttributeCache.ShaderAttributeInfo attribute,
    out MyArgs args)
{
    var tokens = ShaderAttributeArgumentParser.Split(attribute.Args);
    args = new MyArgs(tokens);
    return true;
}
```

## When To Use This Base Class

Use `ShaderPropertyRenderer<TArgs>` when:

- the renderer exists for one attribute,
- matching depends on that attribute being present,
- arguments are simple enough to parse once per draw,
- you want `CanRender` implemented for you.

Use raw `IMaterialGUIPropertyRenderer` when:

- matching depends on several unrelated conditions,
- the renderer is not driven by one attribute,
- you need complete control over filtering.
