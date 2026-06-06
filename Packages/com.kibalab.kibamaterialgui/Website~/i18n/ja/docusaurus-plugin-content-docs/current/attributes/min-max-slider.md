# MinMaxSlider

![MinMaxSlider の例](/img/attributes/attribute-min-max-slider.png)

`[MinMaxSlider]` は Min/Max 範囲を編集する UI を提供します。

```shaderlab
[MinMaxSlider(0, 1)] _ThresholdRange ("Threshold Range", Vector) = (0.2,0.8,0,0)
```

```shaderlab
[MinMaxSlider(min, max)]
```

Vector の `x` と `y` を最小/最大値として使う範囲設定に向いています。
