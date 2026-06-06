# Unity 組み込み Attribute

![Unity 組み込み Attribute の例](/img/attributes/attribute-unity-built-ins.png)

KIBAMaterialGUI は Unity 標準の ShaderLab Attribute と併用できます。

```shaderlab
[Toggle] _UseEmission ("Use Emission", Float) = 0
[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
[NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
```

よく使う Attribute は `[Toggle]`、`[Enum]`、`[KeywordEnum]`、`[PowerSlider]`、`[NoScaleOffset]`、`[Normal]` です。

```shaderlab
[Group(Surface)] [NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
```
