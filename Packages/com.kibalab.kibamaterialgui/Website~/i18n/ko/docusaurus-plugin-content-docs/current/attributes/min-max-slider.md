# MinMaxSlider

![MinMaxSlider 예시](/img/attributes/attribute-min-max-slider.png)

`[MinMaxSlider]`는 범위 값을 Min/Max UI로 편집할 수 있게 합니다.

```shaderlab
[MinMaxSlider(0, 1)] _ThresholdRange ("Threshold Range", Vector) = (0.2,0.8,0,0)
```

## 문법

```shaderlab
[MinMaxSlider(min, max)]
```

## 권장 사용법

- Vector의 `x`, `y`를 min/max로 사용하는 범위 값
- 마스크, 페이드, 거리 범위 같은 쌍 값
- 사용자가 숫자를 직접 입력하면서 슬라이더로도 조정해야 하는 값
