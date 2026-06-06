# Matching And Priority

KIBAMaterialGUI can have many renderers available at the same time. `MaterialGUIPropertyRendererAttribute` tells it which renderer is eligible for a property.

## Basic Match

```csharp
[MaterialGUIPropertyRenderer(
    Order = -100,
    PropertyTypes = new[] { MaterialProperty.PropType.Float },
    RequireShaderAttributes = new[] { "StepFloat" })]
```

This renderer matches only float properties that have `[StepFloat]`.

## Available Filters

| Filter | Use |
| --- | --- |
| `Order` | Lower values resolve first. |
| `ShaderNameEquals` | Match exact shader names. |
| `ShaderNameContains` | Match shader name substrings. |
| `ShaderNameRegex` | Match shader names with a regex. |
| `RequireProperties` | Require other material properties to exist. |
| `RequireKeywords` | Require material keywords to be enabled. |
| `PropertyName` | Match one property name. |
| `PropertyTypes` | Match one or more Unity property types. |
| `RequireShaderAttributes` | Require ShaderLab attributes. |
| `ExcludeShaderAttributes` | Reject ShaderLab attributes. |

## Shader-Specific Renderer

```csharp
[MaterialGUIPropertyRenderer(
    Order = -200,
    ShaderNameEquals = new[] { "KIBA_/Examples/Character" },
    PropertyName = "_SkinMask",
    PropertyTypes = new[] { MaterialProperty.PropType.Texture },
    RequireShaderAttributes = new[] { "SkinMask" })]
```

Use this when the renderer only makes sense for one shader family.

## Attribute-Specific Renderer

```csharp
[MaterialGUIPropertyRenderer(
    Order = -150,
    PropertyTypes = new[] { MaterialProperty.PropType.Vector },
    RequireShaderAttributes = new[] { "ChannelMask" },
    ExcludeShaderAttributes = new[] { "Vector2", "Vector3" })]
```

Use this for reusable UI hints that shader authors can opt into.

## Runtime Filter

Implement `IMaterialGUIPropertyRendererFilter` when the decision needs code.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

[MaterialGUIPropertyRenderer(
    Order = -120,
    PropertyTypes = new[] { MaterialProperty.PropType.Float })]
public sealed class KeywordControlledRenderer :
    IMaterialGUIPropertyRenderer,
    IMaterialGUIPropertyRendererFilter
{
    public bool CanRender(PropertyRendererArgs args)
        => args.Material != null &&
           args.Material.IsKeywordEnabled("_SPECIAL_MODE");

    public float GetHeight(PropertyRendererArgs args)
        => EditorGUIUtility.singleLineHeight;

    public Rect OnGUI(PropertyRendererArgs args)
        => args.Position;
}
```

## Priority Rules

- KIBAMaterialGUI scans all renderer types with `MaterialGUIPropertyRendererAttribute`.
- Invalid renderer declarations are ignored.
- Matching renderers are sorted by `Order`.
- The first renderer that passes static filters and `CanRender` is used.

Use negative `Order` values for custom renderers that should override KIBAMaterialGUI's built-in choices. Use higher values for fallback-style renderers.

## Debugging Match Problems

Check:

- the renderer class is public and not abstract,
- the script is in an Editor assembly,
- the ShaderLab bridge drawer exists,
- `RequireShaderAttributes` matches the attribute name without brackets,
- `PropertyTypes` includes the actual property type,
- `ShaderNameEquals` uses the shader name, not the file name.
