# FlexibleRange

![FlexibleRange 범위 안 예시](/img/attributes/attribute-flexible-range-in-range.png)

![FlexibleRange 범위 밖 예시](/img/attributes/attribute-flexible-range-out-of-range.png)

`[FlexibleRange]`는 일반 `[Range]`보다 느슨한 숫자 편집을 제공할 때 사용합니다.

```shaderlab
[FlexibleRange(0, 1)] _Strength ("Strength", Float) = 0.5
```

## 사용 시점

- 보통은 권장 범위 안에서 편집하지만, 예외적으로 범위 밖 값도 허용해야 할 때
- 디버그나 스타일라이즈드 셰이더처럼 극단값 실험이 필요한 경우

`Float` 프로퍼티에 권장 범위를 지정할 수 있고, `Range` 프로퍼티에도 사용할 수 있습니다. 값이 권장 범위 안에 있을 때는 슬라이더로 표시되고, 범위 밖 값이 들어오면 해당 행은 숫자 입력 필드로 전환되어 범위 밖 상태가 명확하게 보입니다.

음수 범위는 ShaderLab 파서 호환을 위해 `-1` 대신 `n1`, `-0.5` 대신 `n0_5`처럼 씁니다.

```shaderlab
[Group(Lighting)] [FlexibleRange(n1, 1)] _Contrast ("Contrast", Float) = 0
```
