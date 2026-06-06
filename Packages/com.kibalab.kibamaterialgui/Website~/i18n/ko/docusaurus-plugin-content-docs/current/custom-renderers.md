# 커스텀 렌더러

커스텀 렌더러는 특정 머티리얼 프로퍼티 한 줄을 직접 그릴 때 사용합니다.

예를 들어 `[StepFloat]`가 붙은 Float를 정수 단계로만 편집하게 하거나, `[Vector]`가 붙은 Vector를 프로젝트 전용 레이아웃으로 표시할 수 있습니다.

## 언제 사용하나요?

- 프로퍼티 하나의 필드 UI를 바꿔야 할 때
- ShaderLab Attribute를 기준으로 렌더러를 선택하고 싶을 때
- Undo, animation recording, mixed value 표시를 유지하면서 커스텀 IMGUI를 그려야 할 때

그룹 헤더, 툴바, 여러 프로퍼티를 한 번에 조작하는 UI는 [에디터 인젝션](./editor-injection.md)을 사용하세요.

## 기본 흐름

1. ShaderLab이 Attribute를 인식하도록 drawer bridge를 만듭니다.
2. `IMaterialGUIPropertyRenderer`를 구현합니다.
3. `MaterialGUIPropertyRendererAttribute`로 매칭 조건을 지정합니다.
4. 값 쓰기는 `PropertyRendererArgs`의 helper를 사용합니다.

자세한 구현은 [첫 렌더러 만들기](./custom-renderers/first-renderer.md)부터 보세요.

