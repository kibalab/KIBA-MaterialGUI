# カスタムレンダラー

カスタムレンダラーは、特定のマテリアルプロパティ1行の描画を差し替えるための API です。

例として、`[StepFloat]` が付いた Float を整数ステップだけで編集したり、`[Vector]` が付いた Vector をプロジェクト専用のレイアウトで表示できます。

## 使う場面

- プロパティ1つのフィールド UI を変えたい
- ShaderLab Attribute に応じてレンダラーを選びたい
- Undo、animation recording、mixed value 表示を保ったまま IMGUI を描きたい

ツールバー、グループ、複数プロパティを扱う UI には [エディター注入](./editor-injection.md) を使います。

## 基本手順

1. ShaderLab が Attribute を認識するように drawer bridge を作ります。
2. `IMaterialGUIPropertyRenderer` を実装します。
3. `MaterialGUIPropertyRendererAttribute` でマッチ条件を指定します。
4. 値の書き込みは `PropertyRendererArgs` の helper を使います。

