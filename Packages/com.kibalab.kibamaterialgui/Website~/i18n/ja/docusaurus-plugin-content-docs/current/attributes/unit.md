# Unit

![Unit の例](/img/attributes/attribute-unit.png)

`[Unit]` は数値フィールドの横に単位を表示します。

```shaderlab
[Unit(m)] _Distance ("Distance", Float) = 1
[Unit(deg)] _Angle ("Angle", Float) = 45
```

単位は表示ヒントです。値の変換は行いません。
