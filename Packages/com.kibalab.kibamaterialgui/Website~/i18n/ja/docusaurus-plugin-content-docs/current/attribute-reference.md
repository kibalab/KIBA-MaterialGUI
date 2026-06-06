# Attribute リファレンス

KIBAMaterialGUI は ShaderLab Attribute-first の方針です。表示や拡張は displayName DSL ではなく Attribute で表現します。

```shaderlab
[Group(Surface)] [Unit(cd)] _Intensity ("Intensity", Float) = 1
```

## 組み込み Attribute

| Attribute | 用途 |
| --- | --- |
| `[Group]` | プロパティをグループへ配置します。 |
| `[ShowIf]` | 他の数値プロパティに応じて表示を切り替えます。 |
| `[Vector]` | Vector の各成分に意味のあるラベルを付けます。 |
| `[MinMaxSlider]` | Min/Max 範囲を編集します。 |
| `[Unit]` | 数値の単位を表示します。 |
| `[Space]` | 余白を追加します。 |
| `[Divider]` | 区切り線を追加します。 |
| `[GradientTexture]` | グラデーションテクスチャ用の表示にします。 |
| `[FlexibleRange]` | 柔軟な数値範囲 UI を提供します。 |
| `[SegmentedEnum]` | enum をセグメントボタンで表示します。 |
| `[Validate]` | 値の検証警告を表示します。 |

詳細は各 Attribute ページを参照してください。

