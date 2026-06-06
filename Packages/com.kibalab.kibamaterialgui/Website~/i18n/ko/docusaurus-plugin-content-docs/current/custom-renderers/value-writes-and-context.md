# 값 쓰기와 Context

커스텀 렌더러에서 가장 중요한 규칙은 머티리얼 값을 KIBAMaterialGUI helper로 쓰는 것입니다.

## 사용해야 하는 메서드

```csharp
args.SetFloatValue(value, "Change Float");
args.SetColorValue(color, "Change Color");
args.SetVectorValue(vector, "Change Vector");
args.SetTextureValue(texture, "Change Texture");
args.SetTextureScaleAndOffset(scaleOffset, "Change Texture Scale");
args.RegisterPropertyValueChange("Change Property");
```

이 helper들은 Unity Undo, `GUI.changed`, 애니메이션 녹화 흐름을 함께 처리합니다.

## 직접 쓰면 안 되는 예

```csharp
args.Property.floatValue = next;
```

이 방식은 값은 바뀔 수 있지만 Undo, animation recording, 다중 선택 동기화가 깨질 수 있습니다.

## 유용한 Context 정보

`PropertyRendererArgs`에서 자주 쓰는 값은 다음과 같습니다.

| 멤버 | 설명 |
| --- | --- |
| `MaterialEditor` | 현재 Unity `MaterialEditor` |
| `Material` | 대표 머티리얼 |
| `Property` | 현재 `MaterialProperty` |
| `Label` | 현지화된 표시 라벨 |
| `Model` | 그룹, 진단, 조건부 표시 정보 |
| `Layout` | 행 내부 rect 정보 |

