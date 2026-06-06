# 첫 렌더러 만들기

이 예시는 `[StepFloat]`가 붙은 Float/Range 프로퍼티를 반올림해서 저장하는 렌더러를 만듭니다.

## 1. ShaderLab bridge 추가

Unity는 알 수 없는 ShaderLab Attribute를 먼저 거부할 수 있습니다. Attribute 이름과 같은 drawer 클래스를 Editor 어셈블리에 추가합니다.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class StepFloatDrawer : ShaderPropertyDrawer
{
    public StepFloatDrawer() {}
}
```

이 클래스는 필드를 그리지 않습니다. `[StepFloat]` 문법을 Unity가 받아들이게 하는 역할만 합니다.

## 2. 렌더러 구현

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

## 3. 쉐이더에서 사용

```shaderlab
[Group(Main)] [StepFloat] _Intensity ("Intensity", Float) = 1
```

## 핵심 규칙

- 값을 쓸 때 `args.Property.floatValue = ...`처럼 직접 쓰지 않습니다.
- `args.SetFloatValue`를 사용해야 Undo와 애니메이션 녹화가 정상 동작합니다.
- 타입 조건과 Attribute 조건을 함께 지정해 잘못된 프로퍼티에 매칭되지 않게 합니다.

