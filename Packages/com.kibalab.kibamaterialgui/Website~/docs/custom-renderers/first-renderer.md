# Your First Renderer

This page builds the smallest useful custom renderer: a float field that appears only when a shader property has `[StepFloat]`.

## Goal

Shader authors can write:

```shaderlab
[StepFloat] _Intensity ("Intensity", Float) = 1
```

and the property is drawn by your C# renderer instead of KIBAMaterialGUI's default float renderer.

## 1. Add A ShaderLab Bridge

Unity needs a matching drawer class before it accepts custom material attributes.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class StepFloatDrawer : ShaderPropertyDrawer
{
    public StepFloatDrawer() {}
}
```

This class only tells Unity that `[StepFloat]` is valid ShaderLab syntax. It does not draw the field.

## 2. Add The Renderer

```csharp
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

[MaterialGUIPropertyRenderer(
    Order = -100,
    PropertyTypes = new[] { MaterialProperty.PropType.Float, MaterialProperty.PropType.Range },
    RequireShaderAttributes = new[] { "StepFloat" })]
public sealed class StepFloatRenderer : IMaterialGUIPropertyRenderer
{
    public float GetHeight(PropertyRendererArgs args)
        => EditorGUIUtility.singleLineHeight;

    public Rect OnGUI(PropertyRendererArgs args)
    {
        EditorGUI.BeginChangeCheck();
        var next = EditorGUI.FloatField(args.Position, args.Label, args.Property.floatValue);
        if (EditorGUI.EndChangeCheck())
            args.SetFloatValue(Mathf.Round(next));

        return args.Position;
    }
}
```

## 3. Put It In An Editor Assembly

Place the script under an `Editor` folder or an Editor-only asmdef.

KIBAMaterialGUI discovers renderer classes through Unity `TypeCache`; you do not need to register them manually.

## 4. Use It In A Shader

```shaderlab
Shader "KIBA_/Examples/StepFloat"
{
    Properties
    {
        [Group(Main)] [StepFloat] _Intensity ("Intensity", Float) = 1
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
    SubShader { Pass {} }
}
```

## What Makes It Work

- `StepFloatDrawer` lets ShaderLab parse `[StepFloat]`.
- `MaterialGUIPropertyRendererAttribute` tells KIBAMaterialGUI when the renderer can be used.
- `RequireShaderAttributes = new[] { "StepFloat" }` ties the renderer to the ShaderLab attribute.
- `args.SetFloatValue` writes the value through KIBAMaterialGUI so undo and animation recording work.

## Common Mistakes

- Missing drawer class: Unity ignores or rejects the custom ShaderLab attribute.
- Directly assigning `args.Property.floatValue`: undo and animation recording can be inconsistent.
- Forgetting `PropertyTypes`: the renderer may match property types it cannot safely draw.
- Using this for toolbar/group UI: use editor injection instead.
