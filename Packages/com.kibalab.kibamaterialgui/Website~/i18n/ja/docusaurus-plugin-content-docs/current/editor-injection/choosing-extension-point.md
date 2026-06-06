# 拡張ポイントの選び方

目的に合わせて API を選びます。

| 目的 | API |
| --- | --- |
| プロパティフィールドを変える | `IMaterialGUIPropertyRenderer` |
| ツールバーにボタンを追加 | `IToolbarContributor` |
| グループヘッダーボタンを追加 | `IMaterialGUIGroupActionContributor` |
| グループ右クリックメニューを追加 | `IGroupMenuContributor` |
| 決まった位置に IMGUI を追加 | `IShaderEditor` |
| 警告/エラーを表示 | `IMaterialGUIDiagnosticProvider` |
| カスタムフィルターを追加 | `IMaterialGUIFilterProvider` |

レンダラーはプロパティ行用、注入 API はインスペクター周辺 UI 用です。

