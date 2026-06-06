**한국어** | [English](README.en.md) | [日本語](README.ja.md)

---

# KIBAMaterialGUI

KIBAMaterialGUI는 ShaderLab Attribute로 Unity 머티리얼 인스펙터를 구성하는 에디터 전용 `ShaderGUI` 패키지입니다.

셰이더 작성자는 별도 커스텀 에디터를 직접 작성하지 않고도 그룹, 검색, 조건부 표시, 검증, 프리셋, 현지화, 커스텀 렌더러 확장을 사용할 수 있습니다.

## 설치

VCC에 KIBALAB VPM listing을 추가한 뒤 `KIBAMaterialGUI`를 설치합니다.

```text
https://vpm.kiba.red/vcc
```

직접 Unity 프로젝트에 임베드하려면 이 저장소의 패키지 폴더를 복사합니다.

```text
Packages/com.kibalab.kibamaterialgui
```

## 빠른 시작

셰이더에 커스텀 에디터를 지정합니다.

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

ShaderLab `Properties`에 Attribute를 추가합니다.

```shaderlab
Properties
{
    [Group(Basics)] _MainTex ("Main Texture", 2D) = "white" {}
    [Group(Basics)] _Tint ("Tint", Color) = (1,1,1,1)

    [Group(Advanced,Lighting)][Toggle] _LightingToggle ("Lighting", Float) = 0
    [Group(Advanced,Lighting)][ShowIf(_LightingToggle, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
}
```

## 주요 Attribute

- `[Group(...)]`: 프로퍼티를 그룹과 중첩 그룹으로 배치합니다.
- `[ShowIf(...)]`: 다른 numeric 프로퍼티 값에 따라 표시 여부를 제어합니다.
- `[Vector(...)]`: Vector 필드의 컴포넌트 표시 방식을 제어합니다.
- `[MinMaxSlider(...)]`: Vector 프로퍼티를 min/max 슬라이더로 표시합니다.
- `[FlexibleRange(...)]`: 권장 범위 밖 값을 허용하는 float/range UI를 제공합니다.
- `[Unit(...)]`, `[Space(...)]`, `[Divider]`: 인스펙터 표시 힌트를 제공합니다.
- `[SegmentedEnum]`, `[GradientTexture]`, `[Validate(...)]`: enum, gradient texture, 검증 흐름을 확장합니다.

Unity 기본 Attribute인 `[Enum]`, `[KeywordEnum]`, `[Toggle]`, `[ToggleOff]`, `[HDR]`, `[NoScaleOffset]`도 함께 사용할 수 있습니다.

## 문서

문서는 GitHub Pages에 배포됩니다.

```text
https://kibalab.github.io/KIBA-MaterialGUI/
```

패키지 내부의 `Website~/docs`에도 quickstart, Attribute reference, custom renderer, editor injection, scripting API 문서가 포함되어 있습니다.

## 배포 설정

이 저장소는 KIBALAB VPM 패키지 템플릿의 릴리스 워크플로우를 사용합니다.

| 항목 | 값 |
| --- | --- |
| Repository Variable `PACKAGE_NAME` | `com.kibalab.kibamaterialgui` |
| Repository Variable `VPM_BACKEND_URL` | `https://vpm.kiba.red` |
| Repository Secret `VPM_API_KEY` | VPM 백엔드 API 키 |

`release.yml`은 `PACKAGE_NAME`이 없을 때도 `com.kibalab.kibamaterialgui`를 기본값으로 사용합니다.

`deploy-docs.yml`은 `release` 브랜치에 포함된 릴리스 태그가 푸시될 때 Docusaurus 문서를 GitHub Pages로 배포합니다.

## 릴리스

`main`은 개발 브랜치입니다. 릴리스할 때는 `release` 브랜치를 `main`으로 fast-forward 또는 merge한 뒤, `release` 브랜치의 커밋에 `package.json`의 `version`과 같은 Git 태그를 붙여 푸시합니다.

```bash
git switch release
git pull origin release
git merge --ff-only origin/main
git push origin release
git tag 0.1.0
git push origin 0.1.0
```

## 라이선스

MIT
