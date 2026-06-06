---
slug: /
---

# KIBAMaterialGUI

KIBAMaterialGUI は、ShaderLab Attribute を使って Unity のマテリアルインスペクターをグループ化、検索、拡張できる汎用 ShaderGUI パッケージです。

シェーダー作者は `[Group]`、`[ShowIf]`、`[Vector]`、`[MinMaxSlider]`、`[Validate]` などをプロパティに付けるだけで、編集者にとって扱いやすいインスペクターを提供できます。

## 主な機能

- パスベースのグループ表示
- プロパティ名、ラベル、グループパス、Attribute 名の検索
- 変更値、警告、型によるフィルター
- `[ShowIf]` による条件付き表示
- Unity の Undo、アニメーション記録、プレビューとの連携
- カスタムレンダラーとエディター注入 API

## 最小例

```shaderlab
Shader "KIBA_/Examples/Simple"
{
    Properties
    {
        [Group(Main)] _Color ("Color", Color) = (1,1,1,1)
        [Group(Main)] [Range(0, 1)] _Intensity ("Intensity", Float) = 1
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
    SubShader { Pass {} }
}
```

まずは [クイックスタート](./quick-start.md) から始めてください。

