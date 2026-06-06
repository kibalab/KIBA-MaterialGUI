# Attribute 레퍼런스

KIBAMaterialGUI은 ShaderLab Attribute-first 정책을 사용합니다. 프로퍼티 표시 방식과 도구 확장은 displayName DSL이 아니라 Attribute로 표현합니다.

## 기본 구조

```shaderlab
[Group(Surface)] [Unit(cd)] _Intensity ("Intensity", Float) = 1
```

Unity가 Attribute를 파싱하려면 해당 Attribute 이름의 drawer가 패키지 또는 프로젝트에 있어야 합니다. 패키지 내장 Attribute는 별도 작업 없이 사용할 수 있습니다.

## 내장 Attribute

| Attribute | 용도 |
| --- | --- |
| `[Group]` | 프로퍼티를 그룹 경로에 배치합니다. |
| `[ShowIf]` | 다른 숫자 프로퍼티 값에 따라 표시 여부를 제어합니다. |
| `[Vector]` | Vector 프로퍼티를 의미 있는 컴포넌트 라벨로 표시합니다. |
| `[MinMaxSlider]` | Vector/Float 범위를 Min/Max 슬라이더로 편집합니다. |
| `[Unit]` | 숫자 필드 옆에 단위 텍스트를 표시합니다. |
| `[Space]` | 프로퍼티 사이에 여백을 추가합니다. |
| `[Divider]` | 구분선을 추가합니다. |
| `[GradientTexture]` | 텍스처 프로퍼티를 그라디언트 텍스처 워크플로로 표시합니다. |
| `[FlexibleRange]` | 일반 Range보다 유연한 숫자 편집 UI를 제공합니다. |
| `[SegmentedEnum]` | enum 값을 세그먼트 버튼으로 표시합니다. |
| `[Validate]` | 값 검증 경고를 표시합니다. |

각 Attribute의 자세한 문법은 하위 페이지를 확인하세요.

