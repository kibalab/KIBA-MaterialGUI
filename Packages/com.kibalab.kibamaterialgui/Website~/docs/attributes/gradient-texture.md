# GradientTexture

![GradientTexture example](/img/attributes/attribute-gradient-texture.png)

![GradientTexture editor example](/img/attributes/attribute-gradient-texture-editor.png)

`[GradientTexture]` displays a 2D texture property as a gradient editor.

```shaderlab
[GradientTexture] _Ramp ("Ramp", 2D) = "white" {}
```

## Syntax

```shaderlab
[GradientTexture]
[Gradient]
```

## Behavior

- Intended for `2D` texture properties.
- KIBAMaterialGUI stores gradient metadata next to the material and bakes a texture for shader use.
- Texture properties ending in `_gradient` or `_gradienttex` are also treated as gradient candidates.
- The material still receives a texture value, so shader code reads it as a normal texture.

## Example

```shaderlab
Properties
{
    [Group(Color)] [GradientTexture] _ColorRamp ("Color Ramp", 2D) = "white" {}
}
```
