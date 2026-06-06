# 문제 해결

## Attribute가 적용되지 않음

쉐이더 프로퍼티 Attribute는 Unity가 먼저 파싱합니다. 커스텀 Attribute를 만들었다면 `{AttributeName}Drawer` 클래스를 Editor 어셈블리에 추가했는지 확인하세요.

## 그룹이 예상과 다르게 보임

`[Group]` 경로는 쉼표로 중첩을 표현합니다.

```shaderlab
[Group(Surface, Detail)]
```

`/`는 사용하지 마세요.

## 값 변경이 Undo나 애니메이션 녹화에 남지 않음

커스텀 렌더러에서 `MaterialProperty` 값을 직접 쓰지 말고 `PropertyRendererArgs`의 `SetFloatValue`, `SetColorValue`, `SetVectorValue`, `SetTextureValue` 또는 `RegisterPropertyValueChange`를 사용하세요.

## 조건부 표시가 동작하지 않음

`[ShowIf]` 컨트롤러 프로퍼티가 같은 쉐이더에 존재하고 숫자 타입인지 확인하세요. 비교는 float 근사값으로 수행됩니다.

## 검색 결과에 프로퍼티가 없음

검색어가 맞아도 `[ShowIf]` 조건이 false이면 프로퍼티는 표시되지 않습니다.

