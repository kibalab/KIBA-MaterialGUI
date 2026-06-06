# Validate

![Validate 예시](/img/attributes/attribute-validate.png)

`[Validate]`는 값이 프로젝트나 쉐이더가 기대하는 범위를 벗어났을 때 경고를 표시하기 위한 Attribute입니다.

```shaderlab
[Validate(Min, 0)] _Intensity ("Intensity", Float) = 1
```

## 사용 예

- 최소/최대값 경고
- 특정 조합에서만 허용되는 값
- 텍스처 누락 경고

검증은 값을 강제로 바꾸지 않습니다. 머티리얼 작성자가 문제를 알 수 있도록 진단을 표시하는 용도입니다.

## 커스텀 검증

프로젝트 전용 검증은 `IMaterialGUIDiagnosticProvider`로 추가할 수 있습니다.
