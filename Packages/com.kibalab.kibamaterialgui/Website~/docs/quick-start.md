# Quick Start

This guide shows the shortest path from a normal Unity shader to a KIBAMaterialGUI material inspector.

## 1. Add The Package

Add the package to your Unity project. During local development, a file dependency can point at the package folder:

```json
{
  "dependencies": {
    "com.kibalab.kibamaterialgui": "file:../com.kibalab.kibamaterialgui"
  }
}
```

## 2. Enable The ShaderGUI

Add `CustomEditor` at the shader root:

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

The string must match the package editor type exactly.

## 3. Group Properties

Use `[Group]` on properties you want to show under the same section:

```shaderlab
Properties
{
    [Group(Base)] _MainTex ("Main Texture", 2D) = "white" {}
    [Group(Base)] _BaseColor ("Base Color", Color) = (1,1,1,1)

    [Group(Emission)] [Toggle] _UseEmission ("Use Emission", Float) = 0
    [Group(Emission)] [ShowIf(_UseEmission, 1)] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
}
```

Nested groups use comma-separated arguments:

```shaderlab
[Group(Surface, Detail)] _DetailMask ("Detail Mask", 2D) = "white" {}
```

Do not use slash-separated paths. ShaderLab attribute parsing does not support that form reliably.

## 4. Use Inspector Hints

Attributes can change how a property is displayed:

```shaderlab
[Group(Shape)] [Vector(2)] _TilingOffset ("Tiling Offset", Vector) = (1,1,0,0)
[Group(Shape)] [MinMaxSlider(0, 1)] _HeightRange ("Height Range", Vector) = (0.2,0.8,0,0)
[Group(Shape)] [Unit(m)] _Distance ("Distance", Float) = 2
```

Unity attributes such as `[Toggle]`, `[ToggleOff]`, `[Enum]`, `[KeywordEnum]`, `[HDR]`, and `[NoScaleOffset]` continue to work.

## 5. Open A Material

Create a material with your shader. The inspector will show:

- grouped sections,
- a search box,
- a filter dropdown,
- changed-value indicators,
- reset buttons,
- diagnostics for malformed KIBAMaterialGUI attributes.

## Complete Example

```shaderlab
Shader "KIBA_/Examples/QuickStart"
{
    Properties
    {
        [Group(Base)] [NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
        [Group(Base)] _BaseColor ("Base Color", Color) = (1,1,1,1)

        [Group(Lighting)] [Toggle] _Lighting ("Lighting", Float) = 1
        [Group(Lighting)] [ShowIf(_Lighting, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)

        [Group(Advanced, UV)] [Vector(4)] _UV ("UV Transform", Vector) = (1,1,0,0)
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"

    SubShader
    {
        Pass {}
    }
}
```
