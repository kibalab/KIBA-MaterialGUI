# ShowIf

![ShowIf 表示状態 の例](/img/attributes/attribute-show-if-visible.png)

![ShowIf 非表示状態 の例](/img/attributes/attribute-show-if-hidden.png)

`[ShowIf]` は、別の数値プロパティが期待値と一致するときだけプロパティを表示します。

```shaderlab
[Toggle] _UseRim ("Use Rim", Float) = 0
[ShowIf(_UseRim, 1)] _RimColor ("Rim Color", Color) = (0.4,0.7,1,1)
```

## 構文

```shaderlab
[ShowIf(_Controller)]
[ShowIf(_Controller, value)]
```

## 挙動

- `[ShowIf(_Controller)]` は `_Controller == 1` と同じです。
- enum の backing float にも使えます。
- 比較は近似 float 比較です。
- 複数マテリアル選択では、1つでも条件を満たせば表示されます。
- 条件で非表示のプロパティは検索やフィルターでも表示されません。

## 診断

コントローラーが存在しない、数値型ではない、引数が不正な場合は診断に表示されます。
