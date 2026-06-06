# GradientTexture

![GradientTexture 예시](/img/attributes/attribute-gradient-texture.png)

![GradientTexture 에디터 예시](/img/attributes/attribute-gradient-texture-editor.png)

`[GradientTexture]`는 텍스처 프로퍼티를 그라디언트 기반 워크플로로 다룰 때 사용합니다.

```shaderlab
[GradientTexture] _Ramp ("Ramp", 2D) = "white" {}
```

## 용도

- toon ramp
- 색상 보정 lookup
- 사용자 편집용 1D gradient texture

프로젝트별 생성 규칙이 필요하면 커스텀 렌더러나 에디터 인젝션으로 그라디언트 생성 UI를 확장할 수 있습니다.
