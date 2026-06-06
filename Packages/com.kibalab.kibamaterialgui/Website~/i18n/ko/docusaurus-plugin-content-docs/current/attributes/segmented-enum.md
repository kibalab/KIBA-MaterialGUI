# SegmentedEnum

![SegmentedEnum 예시](/img/attributes/attribute-segmented-enum.png)

`[SegmentedEnum]`은 enum float 값을 드롭다운 대신 세그먼트 버튼으로 표시합니다.

```shaderlab
[SegmentedEnum(Off, 0, Add, 1, Multiply, 2)] _BlendMode ("Blend Mode", Float) = 0
```

## 사용 시점

- 선택지가 적고 자주 바뀌는 모드
- 머티리얼 작성자가 현재 값을 한눈에 봐야 하는 설정

선택지가 많다면 Unity 기본 `[Enum]` 드롭다운이 더 적합합니다.
