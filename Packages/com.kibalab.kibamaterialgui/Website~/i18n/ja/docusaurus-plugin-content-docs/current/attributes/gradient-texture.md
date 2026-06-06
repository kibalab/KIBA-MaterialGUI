# GradientTexture

![GradientTexture の例](/img/attributes/attribute-gradient-texture.png)

![GradientTexture エディター の例](/img/attributes/attribute-gradient-texture-editor.png)

`[GradientTexture]` はテクスチャプロパティをグラデーションテクスチャのワークフローとして扱うための Attribute です。

```shaderlab
[GradientTexture] _Ramp ("Ramp", 2D) = "white" {}
```

toon ramp、カラー補正 lookup、ユーザー編集用の 1D gradient texture などに向いています。
