# Unity Built-Ins

![Unity Built-Ins example](/img/attributes/attribute-unity-built-ins.png)

KIBAMaterialGUI keeps common Unity material attributes working and uses their metadata for better controls, search, diagnostics, and filters.

```shaderlab
[Toggle] _Enabled ("Enabled", Float) = 0
[ToggleOff(_DETAIL_OFF)] _Detail ("Detail", Float) = 1
[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
[KeywordEnum(None, Add, Multiply)] _BlendMode ("Blend Mode", Float) = 0
[HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
[NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
```

## Supported Attributes

- `[Toggle]`
- `[ToggleOff]`
- `[Enum]`
- `[KeywordEnum]`
- `[HDR]`
- `[NoScaleOffset]`

## Behavior

- Toggle and keyword attributes use Unity's normal material property conventions.
- Enum metadata can be displayed as a popup or segmented control when combined with `[SegmentedEnum]`.
- `[HDR]` uses Unity's HDR color handling.
- `[NoScaleOffset]` hides texture scale and offset controls for texture properties.

## Example

```shaderlab
Properties
{
    [Group(Main)] [NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
    [Group(Main)] [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
    [Group(Mode)] [KeywordEnum(None, Add, Multiply)] [SegmentedEnum] _Blend ("Blend", Float) = 0
}
```
