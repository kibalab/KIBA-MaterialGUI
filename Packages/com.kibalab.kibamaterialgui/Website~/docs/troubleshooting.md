# Troubleshooting

## The Inspector Does Not Change

Check that the shader contains:

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

Also confirm the package assembly is imported as an Editor assembly.

## An Attribute Is Ignored

Check:

- the attribute name is spelled correctly,
- the property has a matching type,
- the attribute syntax is valid ShaderLab syntax,
- custom attributes have a matching drawer bridge class.

KIBAMaterialGUI does not parse layout commands from display names.

## A Group Does Not Nest

Use comma-separated group arguments:

```shaderlab
[Group(Parent, Child)]
```

Do not use slash-separated paths.

## ShowIf Does Not Work

`ShowIf` only compares numeric material properties.

Check:

- the controller property exists,
- the controller is a Float or Range-like property,
- the expected value is numeric,
- the dependent property is in the same shader.

Malformed conditions appear in diagnostics instead of breaking the inspector.

## Animation Preview Or Recording Looks Wrong

If you wrote a custom renderer, make sure it writes values through `PropertyRendererArgs` helpers. Direct `MaterialProperty` writes can bypass Unity's undo and animation recording paths.

Animation key context menus rely on Unity editor internals. They are best-effort and may vary between Unity versions.

## Localization Or Presets Do Not Load

Check:

- the files are inside a `ShaderPresets` folder next to the shader,
- property names match the shader exactly,
- preset shader regex values match the shader name,
- JSON is valid.
