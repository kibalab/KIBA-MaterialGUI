# SegmentedEnum

![SegmentedEnum の例](/img/attributes/attribute-segmented-enum.png)

`[SegmentedEnum]` は enum float をドロップダウンではなくセグメントボタンで表示します。

```shaderlab
[SegmentedEnum(Off, 0, Add, 1, Multiply, 2)] _BlendMode ("Blend Mode", Float) = 0
```

選択肢が少なく、頻繁に切り替えるモードに向いています。選択肢が多い場合は Unity 標準の `[Enum]` が適しています。
