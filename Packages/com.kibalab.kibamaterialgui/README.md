# KIBAMaterialGUI

KIBAMaterialGUI is an attribute-driven Unity `ShaderGUI` for material inspectors. Shader authors describe layout and editor behavior with ShaderLab attributes, and KIBAMaterialGUI builds a grouped, searchable, resettable inspector from those hints.

This package starts public distribution at version `0.1.0`.

## Install

Install through VCC after adding the KIBALAB VPM listing:

```text
https://vpm.kiba.red/vcc
```

For embedded development, add the package as a local Unity package:

```json
{
  "dependencies": {
    "com.kibalab.kibamaterialgui": "file:../com.kibalab.kibamaterialgui"
  }
}
```

## Quick Start

Add the custom editor to a shader:

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

Use ShaderLab attributes to drive layout and renderer hints:

```shaderlab
Properties
{
    [Group(Basics)] _MainTex ("Main Texture", 2D) = "white" {}
    [Group(Basics)] _Tint ("Tint", Color) = (1,1,1,1)

    [Group(Advanced,Vectors)][Vector(2)] _UV ("UV Transform", Vector) = (1,1,0,0)
    [Group(Advanced,Lighting)][Toggle] _LightingToggle ("Lighting", Float) = 0
    [Group(Advanced,Lighting)][ShowIf(_LightingToggle, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
}
```

Nested group paths use comma-separated `[Group(...)]` arguments. Slash parsing and display-name layout fallback are intentionally not supported.

## Attribute Highlights

- `[Group(Name)]` and `[Group(Parent,Child)]` route properties into groups.
- `[ShowIf(_Prop)]` and `[ShowIf(_Prop, value)]` hide dependent properties until a numeric controller matches.
- `[Vector(2)]`, `[Vector(3)]`, `[Vector(4)]` control vector field count.
- `[MinMaxSlider(min, max)]` renders a vector as a min/max range.
- `[Unit(label)]`, `[Divider]`, `[Space(px)]`, `[SegmentedEnum]`, and `[GradientTexture]` add focused UI hints.
- Unity attributes such as `[Enum]`, `[KeywordEnum]`, `[Toggle]`, `[ToggleOff]`, `[HDR]`, and `[NoScaleOffset]` remain supported.

## Custom Renderers

Custom property renderers are auto-discovered by `TypeCache`:

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

Use `PropertyRendererArgs.SetFloatValue`, `SetColorValue`, `SetVectorValue`, `SetTextureValue`, `SetTextureScaleAndOffset`, or `RegisterPropertyValueChange` before manual writes. These helpers keep undo and Unity animation recording behavior consistent.

Editor-level extensions can also use:

- `IShaderEditor` with `[ShaderEditorInjection(...)]` for low-level hook drawing.
- `IMaterialGUIGroupActionContributor` for group header actions and drag start behavior.
- `IMaterialGUIDiagnosticProvider` for shader/property diagnostics.
- `IMaterialGUIFilterProvider` for custom toolbar filters.

Extension callbacks receive a read-only `MaterialGUIContext`.

## Known Limitations

- Animation keyframe context menu actions rely on Unity editor internals and are best-effort.
- Conditional visibility v1 supports numeric comparisons only.
- This preview release may still make breaking API changes.

See `Website~/docs/` for the quickstart, attribute reference, inspector guide, presets/localization guide, custom renderer guide, editor injection guide, Scripting API, and troubleshooting notes.

## Samples

The package includes `Samples/AllElementsDemo`, a large shader that exercises groups, filters, conditional visibility, presets, localization, validation, custom renderers, and editor contributions.

