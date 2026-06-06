# 最初のレンダラー

この例では、`[StepFloat]` が付いた Float/Range を丸めて保存するレンダラーを作ります。

## 1. ShaderLab bridge

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class StepFloatDrawer : ShaderPropertyDrawer
{
    public StepFloatDrawer() {}
}
```

このクラスはフィールドを描画しません。Unity が `[StepFloat]` を受け入れるための bridge です。

## 2. レンダラー

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
            args.SetFloatValue(Mathf.Round(next), "Set Step Float");

        return args.Position;
    }
}
```

## 3. シェーダーで使う

```shaderlab
[Group(Main)] [StepFloat] _Intensity ("Intensity", Float) = 1
```

値は直接 `args.Property.floatValue` に書かず、必ず `args.SetFloatValue` を使います。

