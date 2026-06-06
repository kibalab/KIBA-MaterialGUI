---
slug: /
---

# KIBAMaterialGUI

KIBAMaterialGUI는 ShaderLab Attribute만으로 Unity 머티리얼 인스펙터를 그룹화하고, 검색하고, 확장할 수 있게 해주는 범용 ShaderGUI 패키지입니다.

쉐이더 작성자는 프로퍼티에 `[Group]`, `[ShowIf]`, `[Vector]`, `[MinMaxSlider]`, `[Validate]` 같은 Attribute를 붙이고, 머티리얼 사용자는 정리된 인스펙터에서 필요한 값을 빠르게 찾고 조정할 수 있습니다.

## 주요 기능

- 프로퍼티를 경로 기반 그룹으로 정리합니다.
- 프로퍼티명, 라벨, 그룹 경로, Attribute 이름으로 검색합니다.
- 변경값, 경고, 타입 필터를 제공합니다.
- `[ShowIf]`로 조건부 표시를 설정합니다.
- Unity 애니메이션 녹화/프리뷰와 Undo 흐름을 유지합니다.
- 커스텀 렌더러와 에디터 인젝션 API로 프로젝트 전용 UI를 추가할 수 있습니다.

## 가장 작은 예시

```shaderlab
Shader "KIBA_/Examples/Simple"
{
    Properties
    {
        [Group(Main)] _Color ("Color", Color) = (1,1,1,1)
        [Group(Main)] [Range(0, 1)] _Intensity ("Intensity", Float) = 1
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
    SubShader { Pass {} }
}
```

다음 단계는 [빠른 시작](./quick-start.md)에서 전체 설정 흐름을 확인하세요.

