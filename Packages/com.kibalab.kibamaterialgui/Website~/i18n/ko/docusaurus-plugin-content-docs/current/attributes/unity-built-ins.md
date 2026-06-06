# Unity 내장 Attribute

![Unity 내장 Attribute 예시](/img/attributes/attribute-unity-built-ins.png)

KIBAMaterialGUI은 Unity의 기본 ShaderLab Attribute와 함께 사용할 수 있습니다.

```shaderlab
[Toggle] _UseEmission ("Use Emission", Float) = 0
[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
[NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
```

## 자주 쓰는 내장 Attribute

- `[Toggle]`
- `[ToggleOff]`
- `[Enum]`
- `[KeywordEnum]`
- `[PowerSlider]`
- `[NoScaleOffset]`
- `[Normal]`

KIBAMaterialGUI Attribute와 Unity Attribute를 함께 붙일 수 있습니다.

```shaderlab
[Group(Surface)] [NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
```
