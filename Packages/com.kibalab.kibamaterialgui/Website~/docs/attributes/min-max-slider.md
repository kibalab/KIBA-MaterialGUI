# MinMaxSlider

![MinMaxSlider example](/img/attributes/attribute-min-max-slider.png)

`[MinMaxSlider]` renders a `Vector` property as a min/max pair with a slider.

```shaderlab
[MinMaxSlider(0, 1)] _HeightRange ("Height Range", Vector) = (0.2,0.8,0,0)
[MinMaxSlider(n1, 1)] _SignedRange ("Signed Range", Vector) = (-0.5,0.5,0,0)
```

## Syntax

```shaderlab
[MinMaxSlider(max)]
[MinMaxSlider(min, max)]
```

## Behavior

- The `x` component stores the minimum value.
- The `y` component stores the maximum value.
- The `z` and `w` components are preserved but not edited by the slider.
- If a range bound is negative, prefix the number with `n`, for example `n1` for `-1`.
- This attribute is intended for `Vector` properties.

## Example

```shaderlab
Properties
{
    [Group(Height)] [MinMaxSlider(0, 4)] _HeightRange ("Height Range", Vector) = (0.25,2,0,0)
}
```
