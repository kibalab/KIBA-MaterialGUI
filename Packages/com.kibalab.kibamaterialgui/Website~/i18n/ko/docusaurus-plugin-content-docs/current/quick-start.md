# 빠른 시작

이 페이지는 패키지를 설치한 뒤 첫 ShaderGUI를 적용하는 최소 절차를 설명합니다.

## 1. 패키지 설치

VPM 또는 Unity Package Manager로 패키지를 프로젝트에 추가합니다. 설치 후 별도 런타임 의존성은 필요하지 않습니다.

## 2. 쉐이더에 CustomEditor 추가

쉐이더 파일에 KIBAMaterialGUI ShaderGUI를 지정합니다.

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

전체 예시는 다음과 같습니다.

```shaderlab
Shader "KIBA_/Examples/QuickStart"
{
    Properties
    {
        [Group(Surface)] _BaseColor ("Base Color", Color) = (1,1,1,1)
        [Group(Surface)] _MainTex ("Main Texture", 2D) = "white" {}

        [Group(Lighting)] [Toggle] _UseLighting ("Use Lighting", Float) = 1
        [Group(Lighting)] [ShowIf(_UseLighting, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
    SubShader { Pass {} }
}
```

## 3. 그룹 경로 지정

`[Group(Surface)]`처럼 그룹 이름을 지정합니다. 중첩 그룹은 쉼표로 나눕니다.

```shaderlab
[Group(Surface, Detail)] _DetailMap ("Detail Map", 2D) = "gray" {}
```

`/`는 ShaderLab 파서에서 안전하지 않으므로 사용하지 않습니다.

## 4. 조건부 표시 추가

다른 숫자 프로퍼티 값에 따라 특정 프로퍼티를 숨기거나 보이게 할 수 있습니다.

```shaderlab
[Toggle] _LightingToggle ("Lighting", Float) = 0
[ShowIf(_LightingToggle, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
```

## 5. 머티리얼 인스펙터 확인

머티리얼을 선택하면 그룹, 검색, 필터, 진단 패널이 활성화됩니다. 조건으로 숨겨진 프로퍼티는 검색 결과에도 나타나지 않습니다.

## 다음에 볼 문서

- [Attribute 레퍼런스](./attribute-reference.md)
- [인스펙터 기능](./inspector-features.md)
- [커스텀 렌더러](./custom-renderers.md)
- [에디터 인젝션](./editor-injection.md)

