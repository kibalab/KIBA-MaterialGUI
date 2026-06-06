# トラブルシューティング

## Attribute が効かない

カスタム Attribute を使う場合は `{AttributeName}Drawer` を Editor アセンブリに追加してください。

## グループが想定どおりにならない

ネストしたグループはカンマで指定します。

```shaderlab
[Group(Surface, Detail)]
```

`/` は使わないでください。

## Undo やアニメーション記録に残らない

カスタムレンダラーで `MaterialProperty` を直接書き換えず、`SetFloatValue`、`SetColorValue`、`SetVectorValue`、`SetTextureValue`、`RegisterPropertyValueChange` を使ってください。

## ShowIf が動かない

コントローラープロパティが同じシェーダーに存在し、数値型であることを確認してください。

