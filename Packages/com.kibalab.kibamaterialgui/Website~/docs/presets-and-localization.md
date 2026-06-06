# Presets And Localization

Presets and localization are optional. They live beside the shader so a shader package can ship inspector labels and starter material values with the shader.

## Folder Layout

Create a `ShaderPresets` folder next to the shader:

```text
MyShaderFolder/
  MyShader.shader
  ShaderPresets/
    ShaderPresets.json
    ShaderLocalization.json
```

KIBAMaterialGUI searches from the shader's folder first.

## Localization Keys

Use `ShaderLocalization.json` to translate UI labels, property names, and group names.

Common key prefixes:

```text
propName:_PropertyName
group:Group/Path
ui:Language
```

Example:

```json
{
  "DefaultCode": "EN",
  "Languages": [
    {
      "Code": "EN",
      "Entries": [
        { "Key": "propName:_BaseColor", "Value": "Base Color" },
        { "Key": "group:Surface", "Value": "Surface" }
      ]
    },
    {
      "Code": "JA",
      "Entries": [
        { "Key": "propName:_BaseColor", "Value": "Base Color" },
        { "Key": "group:Surface", "Value": "Surface" }
      ]
    }
  ]
}
```

Missing keys fall back to the shader display name or the original group path.

## Presets

Presets apply stored values to matching shaders. The easiest way to create a valid preset file is to use the inspector's preset controls and copy the current material as JSON.

Preset groups match shader names by regular expression. Keep expressions specific enough that presets do not appear on unrelated shaders.

## Practical Advice

- Keep localization and preset files in source control with the shader.
- Prefer stable property names. Presets and localization keys are property-name based.
- Use the sample shader as a reference when hand-editing JSON.
