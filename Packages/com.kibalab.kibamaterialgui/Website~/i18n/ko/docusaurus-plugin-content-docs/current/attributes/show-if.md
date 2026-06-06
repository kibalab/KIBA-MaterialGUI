# ShowIf

![ShowIf 조건 만족 예시](/img/attributes/attribute-show-if-visible.png)

![ShowIf 조건 불만족 예시](/img/attributes/attribute-show-if-hidden.png)

`[ShowIf]`는 다른 숫자 프로퍼티 값이 기대값과 같을 때만 프로퍼티를 표시합니다.

```shaderlab
[Toggle] _UseRim ("Use Rim", Float) = 0
[ShowIf(_UseRim, 1)] _RimColor ("Rim Color", Color) = (0.4,0.7,1,1)
```

## 문법

```shaderlab
[ShowIf(_Controller)]
[ShowIf(_Controller, value)]
```

## 동작

- `[ShowIf(_Controller)]`는 `_Controller == 1`과 같습니다.
- enum backing float에도 사용할 수 있습니다.
- 비교는 `Mathf.Approximately` 방식의 근사 비교입니다.
- 다중 머티리얼 선택에서는 하나라도 조건을 만족하면 표시됩니다.
- 조건으로 숨겨진 프로퍼티는 검색/필터로도 표시되지 않습니다.

## 진단

컨트롤러 프로퍼티가 없거나, 숫자 타입이 아니거나, 인자가 잘못된 경우 진단 패널에 경고가 표시됩니다.
