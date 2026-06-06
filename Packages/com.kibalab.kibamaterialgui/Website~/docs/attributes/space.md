# Space

![Space example](/img/attributes/attribute-space.png)

`[Space]` adds vertical spacing before a property row.

```shaderlab
[Space] _SectionStart ("Section Start", Float) = 0
[Space(24)] _WideGap ("Wide Gap", Float) = 0
```

## Syntax

```shaderlab
[Space]
[Space(px)]
```

## Behavior

- Default spacing is used when no value is supplied.
- Values are in editor pixels.
- Spacing applies to the property row that owns the attribute.

## Example

```shaderlab
Properties
{
    [Group(Advanced)] _First ("First", Float) = 0
    [Group(Advanced)] [Space(16)] _Second ("Second", Float) = 0
}
```
