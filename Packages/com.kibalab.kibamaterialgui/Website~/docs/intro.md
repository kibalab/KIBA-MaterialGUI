---
slug: /
---

# KIBAMaterialGUI

KIBAMaterialGUI is a Unity `ShaderGUI` package for shader authors who want a practical material inspector without writing a full custom editor for every shader.

You describe the inspector with ShaderLab attributes next to each property. KIBAMaterialGUI reads those attributes and builds a searchable, grouped, resettable, localized material UI.

## What You Get

- Grouped property sections for large shaders.
- Search across property names, display labels, group paths, and attributes.
- Filters for changed values, warnings, textures, numbers, and colors.
- Conditional visibility with `[ShowIf]`.
- Built-in renderers for toggles, enums, segmented enums, vectors, min/max sliders, gradients, textures, colors, and ranges.
- Presets and localization files that live beside your shader.
- Unity undo, multi-selection, animation preview, and animation recording support where Unity exposes it.

## Minimal Shader

```shaderlab
Shader "KIBA_/Examples/Simple"
{
    Properties
    {
        [Group(Main)] _MainTex ("Main Texture", 2D) = "white" {}
        [Group(Main)] _Tint ("Tint", Color) = (1,1,1,1)
        [Group(Advanced)][Toggle] _UseRim ("Use Rim", Float) = 0
        [Group(Advanced)][ShowIf(_UseRim, 1)] _RimColor ("Rim Color", Color) = (0.4,0.7,1,1)
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"

    SubShader
    {
        Pass {}
    }
}
```

## Where To Go Next

- Start with [Quick Start](./quick-start.md).
- Use [Attribute Reference](./attribute-reference.md) while authoring shader properties.
- Read [Inspector Features](./inspector-features.md) to understand the material editor behavior.
- Add presets and translations with [Presets And Localization](./presets-and-localization.md).
- Use [Custom Renderers](./custom-renderers.md) when a property needs a custom field.
- Use [Editor Injection](./editor-injection.md) when a shader needs small helper UI around groups, properties, or the toolbar.
- Use [Scripting API](./scripting-api.md) as the C# API reference.
