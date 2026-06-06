# 値の書き込みと Context

カスタムレンダラーでは、マテリアル値を KIBAMaterialGUI の helper で書き込みます。

```csharp
args.SetFloatValue(value, "Change Float");
args.SetColorValue(color, "Change Color");
args.SetVectorValue(vector, "Change Vector");
args.SetTextureValue(texture, "Change Texture");
args.SetTextureScaleAndOffset(scaleOffset, "Change Texture Scale");
args.RegisterPropertyValueChange("Change Property");
```

これらは Unity Undo、`GUI.changed`、アニメーション記録をまとめて処理します。

直接代入は避けてください。

```csharp
args.Property.floatValue = next;
```

## よく使う情報

| メンバー | 説明 |
| --- | --- |
| `MaterialEditor` | 現在の Unity `MaterialEditor` |
| `Material` | 代表マテリアル |
| `Property` | 現在の `MaterialProperty` |
| `Label` | ローカライズ済みラベル |
| `Model` | グループ、診断、条件表示情報 |
| `Layout` | 行内の rect 情報 |

