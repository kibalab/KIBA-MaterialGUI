# ShaderLab Attribute Bridge

Unity's material attribute parser is separate from KIBAMaterialGUI. Before KIBAMaterialGUI can read a custom attribute, Unity must accept it.

That is the job of a ShaderLab bridge drawer.

## Naming Rule

For `[MyHint]`, create `MyHintDrawer`.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class MyHintDrawer : ShaderPropertyDrawer
{
    public MyHintDrawer() {}
}
```

Then ShaderLab can use:

```shaderlab
[MyHint] _Amount ("Amount", Float) = 0
```

## Attributes With Arguments

If the ShaderLab attribute has arguments, add constructors matching the forms you want to allow.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class MeterDrawer : ShaderPropertyDrawer
{
    public MeterDrawer() {}
    public MeterDrawer(string label) {}
}
```

```shaderlab
[Meter(cm)] _Length ("Length", Float) = 1
```

The drawer constructor can stay empty. KIBAMaterialGUI reads the raw attribute arguments separately from shader metadata.

## Bridge vs Renderer

The bridge:

- makes ShaderLab accept the attribute,
- does not need to draw UI,
- should be tiny.

The renderer:

- decides whether it matches a property,
- draws the property row,
- writes values through KIBAMaterialGUI helpers.

## Practical Pattern

Keep bridge drawers near your renderers:

```csharp
public sealed class ChannelMaskDrawer : ShaderPropertyDrawer
{
    public ChannelMaskDrawer() {}
}

[MaterialGUIPropertyRenderer(
    PropertyTypes = new[] { MaterialProperty.PropType.Vector },
    RequireShaderAttributes = new[] { "ChannelMask" })]
public sealed class ChannelMaskRenderer : IMaterialGUIPropertyRenderer
{
    // Draw the Vector property as RGBA channel toggles.
}
```

## Built-In Bridge Base

Use:

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;
```

and inherit from:

```csharp
ShaderPropertyDrawer
```

This base class derives from Unity's `MaterialPropertyDrawer` and is meant for KIBAMaterialGUI hint attributes.
