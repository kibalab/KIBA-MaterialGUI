# FlexibleRange

![FlexibleRange 範囲内 の例](/img/attributes/attribute-flexible-range-in-range.png)

![FlexibleRange 範囲外 の例](/img/attributes/attribute-flexible-range-out-of-range.png)

`[FlexibleRange]` は通常の `[Range]` より柔軟な数値編集を提供します。

```shaderlab
[FlexibleRange(0, 1)] _Strength ("Strength", Float) = 0.5
```

普段は推奨範囲内で編集しつつ、必要に応じて範囲外の値も試したい場合に使います。

`Float` プロパティに推奨範囲を指定できます。`Range` プロパティにも使用できます。値が推奨範囲内にある場合はスライダーで表示され、範囲外の値になった場合は数値入力フィールドに切り替わります。

負の範囲は ShaderLab パーサーとの互換性のため、`-1` ではなく `n1`、`-0.5` ではなく `n0_5` のように書きます。

```shaderlab
[Group(Lighting)] [FlexibleRange(n1, 1)] _Contrast ("Contrast", Float) = 0
```
