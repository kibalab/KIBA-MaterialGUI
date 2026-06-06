[한국어](README.md) | [English](README.en.md) | **日本語**

---

# KIBAMaterialGUI

KIBAMaterialGUI は、ShaderLab Attribute から Unity のマテリアルインスペクターを構築する Editor 専用 `ShaderGUI` パッケージです。

シェーダー作者は専用のカスタムエディターを一から書かなくても、グループ、検索、条件付き表示、検証、プリセット、ローカライズ、カスタムレンダラー拡張を利用できます。

## インストール

VCC に KIBALAB VPM listing を追加し、`KIBAMaterialGUI` をインストールします。

```text
https://vpm.kiba.red/vcc
```

埋め込み開発では、このパッケージフォルダーを Unity プロジェクトへコピーします。

```text
Packages/com.kibalab.kibamaterialgui
```

## クイックスタート

シェーダーにカスタムエディターを指定します。

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

ShaderLab の `Properties` に Attribute を追加します。

```shaderlab
Properties
{
    [Group(Basics)] _MainTex ("Main Texture", 2D) = "white" {}
    [Group(Basics)] _Tint ("Tint", Color) = (1,1,1,1)

    [Group(Advanced,Lighting)][Toggle] _LightingToggle ("Lighting", Float) = 0
    [Group(Advanced,Lighting)][ShowIf(_LightingToggle, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
}
```

## 主な Attribute

- `[Group(...)]`: プロパティをグループやネストしたグループへ配置します。
- `[ShowIf(...)]`: 他の numeric プロパティ値に応じて表示を制御します。
- `[Vector(...)]`: Vector フィールドのコンポーネント表示を制御します。
- `[MinMaxSlider(...)]`: Vector プロパティを min/max スライダーとして表示します。
- `[FlexibleRange(...)]`: 意図的な範囲外値を許可する float/range UI を提供します。
- `[Unit(...)]`, `[Space(...)]`, `[Divider]`: インスペクター表示用のヒントを追加します。
- `[SegmentedEnum]`, `[GradientTexture]`, `[Validate(...)]`: enum、gradient texture、検証ワークフローを拡張します。

Unity 標準の `[Enum]`, `[KeywordEnum]`, `[Toggle]`, `[ToggleOff]`, `[HDR]`, `[NoScaleOffset]` もサポートします。

## ドキュメント

ドキュメントは GitHub Pages にデプロイされます。

```text
https://kibalab.github.io/KIBA-MaterialGUI/
```

パッケージ内の `Website~/docs` にも quickstart、attribute reference、custom renderer、editor injection、scripting API ドキュメントが含まれています。

## リリース設定

このリポジトリは KIBALAB VPM パッケージテンプレートのリリースワークフローを使用します。

| 項目 | 値 |
| --- | --- |
| Repository Variable `PACKAGE_NAME` | `com.kibalab.kibamaterialgui` |
| Repository Variable `VPM_BACKEND_URL` | `https://vpm.kiba.red` |
| Repository Secret `VPM_API_KEY` | VPM backend API key |

`release.yml` は `PACKAGE_NAME` が未設定の場合でも `com.kibalab.kibamaterialgui` を既定値として使用します。

`docs.yml` は `main` またはリリースタグが push されたときに Docusaurus ドキュメントを GitHub Pages へデプロイします。

## リリース

Git タグは `package.json` の `version` と一致している必要があります。

```bash
git tag 0.1.0
git push origin 0.1.0
```

## ライセンス

MIT
