# 확장 지점 고르기

어떤 API를 써야 하는지는 "무엇을 바꾸고 싶은지"로 판단합니다.

| 목표 | 사용할 API |
| --- | --- |
| 프로퍼티 필드 UI 변경 | `IMaterialGUIPropertyRenderer` |
| 툴바에 버튼/토글 추가 | `IToolbarContributor` |
| 그룹 헤더 버튼 추가 | `IMaterialGUIGroupActionContributor` |
| 그룹 우클릭 메뉴 추가 | `IGroupMenuContributor` |
| 인스펙터 특정 위치에 IMGUI 추가 | `IShaderEditor` |
| 경고/오류 표시 | `IMaterialGUIDiagnosticProvider` |
| 커스텀 필터 추가 | `IMaterialGUIFilterProvider` |

## 렌더러와 인젝션의 차이

렌더러는 KIBAMaterialGUI이 프로퍼티 행을 그릴 때 호출됩니다. 따라서 값 편집, mixed 표시, animation tint와 함께 동작합니다.

인젝션은 인스펙터 구조 주변에 UI를 추가합니다. 여러 프로퍼티를 한 번에 조작하거나, 설명/도구 버튼을 추가할 때 적합합니다.

