# Attribute Reference

KIBAMaterialGUI is attribute-first. It does not parse layout commands from display names. Put layout, visibility, validation, and renderer hints in ShaderLab attributes next to the property they affect.

## KIBAMaterialGUI Attributes

- [`[Group]`](./attributes/group.md) routes properties into inspector groups.
- [`[ShowIf]`](./attributes/show-if.md) hides a property until another numeric property matches a value.
- [`[Vector]`, `[Vector2]`, `[Vector3]`, `[Vector4]`](./attributes/vector.md) choose how many vector components are shown.
- [`[MinMaxSlider]`](./attributes/min-max-slider.md) renders a vector as a min/max slider pair.
- [`[Unit]`](./attributes/unit.md) shows a compact unit label in numeric fields.
- [`[Space]`](./attributes/space.md) adds vertical spacing before a row.
- [`[Divider]`](./attributes/divider.md) draws a divider before a row.
- [`[GradientTexture]`, `[Gradient]`](./attributes/gradient-texture.md) render a texture as a gradient editor.
- [`[FlexibleRange]`, `[Flexible]`](./attributes/flexible-range.md) mark a range as a flexible numeric control.
- [`[SegmentedEnum]`, `[Segmented]`](./attributes/segmented-enum.md) render enum-like floats as segmented buttons.
- [`[Validate]`](./attributes/validate.md) runs a C# validator when a value changes.

## Unity Attributes

KIBAMaterialGUI also reads common Unity material attributes such as `[Toggle]`, `[ToggleOff]`, `[Enum]`, `[KeywordEnum]`, `[HDR]`, and `[NoScaleOffset]`.

See [Unity Built-Ins](./attributes/unity-built-ins.md).

## General Rules

- Attribute names are case-sensitive in ShaderLab.
- Nested group paths use comma-separated arguments, not slash-separated strings.
- Invalid KIBAMaterialGUI attribute arguments are reported in the inspector diagnostics panel when possible.
- Hidden properties keep their material values unchanged.
