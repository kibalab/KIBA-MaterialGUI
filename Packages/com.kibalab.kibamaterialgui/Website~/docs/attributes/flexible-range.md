# FlexibleRange

![FlexibleRange in range example](/img/attributes/attribute-flexible-range-in-range.png)

![FlexibleRange out of range example](/img/attributes/attribute-flexible-range-out-of-range.png)

`[FlexibleRange]` marks a numeric property as a flexible range control.

```shaderlab
[FlexibleRange(0, 1)] _Intensity ("Intensity", Float) = 0.5
```

## Syntax

```shaderlab
[FlexibleRange(min, max)]
[Flexible]
```

## Behavior

- Works with `Float` properties, and can also be used on `Range` properties.
- The slider stays inside the shader range.
- When the value is outside the recommended range, the row switches to a numeric input field so the out-of-range value is explicit.
- For negative limits, use the ShaderLab-safe `n` prefix syntax, such as `n1` for `-1` or `n0_5` for `-0.5`.
- Use this when a value has a recommended range but authors sometimes need deliberate overdrive values.

## Example

```shaderlab
Properties
{
    [Group(Lighting)] [FlexibleRange(0, 10)] _Intensity ("Intensity", Float) = 1
    [Group(Lighting)] [FlexibleRange(n1, 1)] _Contrast ("Contrast", Float) = 0
}
```
