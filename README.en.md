[한국어](README.md) | **English** | [日本語](README.ja.md)

---

# KIBAMaterialGUI

KIBAMaterialGUI is an editor-only Unity `ShaderGUI` package that builds material inspectors from ShaderLab attributes.

Shader authors can add grouping, search, conditional visibility, validation, presets, localization, and custom renderer extension points without writing a full bespoke material editor.

## Installation

Add the KIBALAB VPM listing to VCC, then install `KIBAMaterialGUI`.

```text
https://vpm.kiba.red/vcc
```

For embedded development, copy this package folder into a Unity project:

```text
Packages/com.kibalab.kibamaterialgui
```

## Quick Start

Assign the custom editor in your shader:

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

Add ShaderLab attributes to `Properties`:

```shaderlab
Properties
{
    [Group(Basics)] _MainTex ("Main Texture", 2D) = "white" {}
    [Group(Basics)] _Tint ("Tint", Color) = (1,1,1,1)

    [Group(Advanced,Lighting)][Toggle] _LightingToggle ("Lighting", Float) = 0
    [Group(Advanced,Lighting)][ShowIf(_LightingToggle, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
}
```

## Attribute Highlights

- `[Group(...)]`: places properties into groups and nested groups.
- `[ShowIf(...)]`: controls visibility from another numeric property.
- `[Vector(...)]`: controls vector component display.
- `[MinMaxSlider(...)]`: renders a vector property as a min/max slider.
- `[FlexibleRange(...)]`: provides a float/range UI that allows deliberate out-of-range values.
- `[Unit(...)]`, `[Space(...)]`, `[Divider]`: add focused inspector display hints.
- `[SegmentedEnum]`, `[GradientTexture]`, `[Validate(...)]`: extend enum, gradient texture, and validation workflows.

Unity built-in attributes such as `[Enum]`, `[KeywordEnum]`, `[Toggle]`, `[ToggleOff]`, `[HDR]`, and `[NoScaleOffset]` are also supported.

## Documentation

Documentation is deployed to GitHub Pages:

```text
https://kibalab.github.io/KIBA-MaterialGUI/
```

The package also includes `Website~/docs` with quickstart, attribute reference, custom renderer, editor injection, and scripting API documentation.

## Release Setup

This repository uses the KIBALAB VPM package template release workflow.

| Item | Value |
| --- | --- |
| Repository Variable `PACKAGE_NAME` | `com.kibalab.kibamaterialgui` |
| Repository Variable `VPM_BACKEND_URL` | `https://vpm.kiba.red` |
| Repository Secret `VPM_API_KEY` | VPM backend API key |

`release.yml` falls back to `com.kibalab.kibamaterialgui` when `PACKAGE_NAME` is not configured.

`docs.yml` deploys the Docusaurus documentation to GitHub Pages when `main` or a release tag is pushed.

## Releasing

The Git tag must match `package.json`'s `version`.

```bash
git tag 0.1.0
git push origin 0.1.0
```

## License

MIT
