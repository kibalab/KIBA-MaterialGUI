# Divider

![Divider example](/img/attributes/attribute-divider.png)

`[Divider]` draws a divider line before a property row.

```shaderlab
[Divider] _AdvancedValue ("Advanced Value", Float) = 0
```

Use it to separate dense groups without creating another group.

## Syntax

```shaderlab
[Divider]
```

## Behavior

- The divider is visual only.
- It does not affect search, filters, reset behavior, or serialization.
- Combine with `[Space]` when a stronger section break is needed.

## Example

```shaderlab
Properties
{
    [Group(Surface)] _BaseColor ("Base Color", Color) = (1,1,1,1)
    [Group(Surface)] [Divider] _DetailStrength ("Detail Strength", Range(0, 1)) = 0.5
}
```
