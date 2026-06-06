# Unit

![Unit example](/img/attributes/attribute-unit.png)

`[Unit]` draws a compact unit label inside a numeric field.

```shaderlab
[Unit(m)] _Distance ("Distance", Float) = 2
[Unit(deg)] _Angle ("Angle", Float) = 45
```

## Syntax

```shaderlab
[Unit(label)]
```

## Behavior

- Works best on `Float` and `Range` properties.
- The unit is visual only. It does not convert values.
- Use short labels so the field remains readable in narrow inspectors.

## Example

```shaderlab
Properties
{
    [Group(Motion)] [Unit(m/s)] _Speed ("Speed", Float) = 3
    [Group(Motion)] [Unit(deg)] _Rotation ("Rotation", Range(0, 360)) = 90
}
```
