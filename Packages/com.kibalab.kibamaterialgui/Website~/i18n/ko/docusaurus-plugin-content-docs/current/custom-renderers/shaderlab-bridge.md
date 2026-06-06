# ShaderLab Bridge

ShaderLab bridge는 Unity가 커스텀 Attribute를 파싱하도록 만드는 최소 drawer 클래스입니다.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class MyHintDrawer : ShaderPropertyDrawer
{
    public MyHintDrawer() {}
}
```

이 예시는 쉐이더에서 `[MyHint]`를 사용할 수 있게 합니다.

```shaderlab
[MyHint] _Value ("Value", Float) = 0
```

## 왜 필요한가요?

KIBAMaterialGUI의 렌더러 선택은 C#에서 이루어지지만, ShaderLab Attribute 파싱은 Unity가 먼저 수행합니다. Unity가 Attribute 자체를 모르면 해당 정보가 KIBAMaterialGUI까지 오지 않습니다.

## 이름 규칙

클래스 이름은 `{AttributeName}Drawer` 형태를 사용합니다.

| ShaderLab | C# drawer |
| --- | --- |
| `[StepFloat]` | `StepFloatDrawer` |
| `[MyHint]` | `MyHintDrawer` |

## 인자를 받는 Attribute

Attribute 인자는 KIBAMaterialGUI의 `ShaderAttributeInfo`에서 읽습니다. Bridge drawer는 보통 빈 생성자만 있으면 충분합니다.

