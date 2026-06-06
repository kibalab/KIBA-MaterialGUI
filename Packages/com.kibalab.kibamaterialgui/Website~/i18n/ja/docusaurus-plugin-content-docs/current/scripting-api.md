# Scripting API

このページは、シェーダーパッケージ作者が主に使う公開 C# API の概要です。すべて Editor 専用です。

## 名前空間

| 名前空間 | 用途 |
| --- | --- |
| `KIBA_.KIBAMaterialGUI.Editor.Core` | モデル、コンテキスト、診断 |
| `KIBA_.KIBAMaterialGUI.Editor.Extensibility` | 注入と contribution API |
| `KIBA_.KIBAMaterialGUI.Editor.ShaderLab` | ShaderLab drawer bridge |
| `KIBA_.KIBAMaterialGUI.Editor.UI.Property` | プロパティレンダラーと値書き込み |

## Context

`MaterialGUIContext` は拡張コールバックに渡される読み取り中心のコンテキストです。

| メンバー | 説明 |
| --- | --- |
| `MaterialEditor` | 現在の Unity `MaterialEditor` |
| `Material` | 代表マテリアル |
| `Targets` | 選択中の全マテリアル |
| `Properties` | 現在の `MaterialProperty` 一覧 |
| `CurrentLanguage` | 現在の表示言語 |
| `Search` | 現在の検索文字列 |
| `Model` | グループ/プロパティモデル |

## Property Renderer

```csharp
public interface IMaterialGUIPropertyRenderer
{
    float GetHeight(PropertyRendererArgs args);
    Rect OnGUI(PropertyRendererArgs args);
}
```

```csharp
[MaterialGUIPropertyRenderer(
    Order = -100,
    PropertyTypes = new[] { MaterialProperty.PropType.Float },
    RequireShaderAttributes = new[] { "StepFloat" })]
```

値の書き込みには helper を使います。

```csharp
args.SetFloatValue(value);
args.SetColorValue(color);
args.SetVectorValue(vector);
args.SetTextureValue(texture);
args.RegisterPropertyValueChange();
```

## Editor Injection

```csharp
public interface IShaderEditor
{
    void OnGUI(InjectionArgs args);
}
```

```csharp
[ShaderEditorInjection(HookPoint.AfterToolbar, Order = 0)]
public sealed class MyInjection : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        EditorGUILayout.LabelField("Project helper");
    }
}
```

## Contribution API

```csharp
public interface IToolbarContributor { void Contribute(ToolbarModel model, InjectionArgs args); }
public interface IGroupMenuContributor { void Contribute(ContextMenuModel model, InjectionArgs args); }
public interface IMaterialGUIGroupActionContributor { void Contribute(GroupActionModel model, InjectionArgs args); }
public interface IMaterialGUIDiagnosticProvider { void Contribute(List<MaterialGUIDiagnostic> diagnostics, InjectionArgs args); }
public interface IMaterialGUIFilterProvider { void Contribute(ToolbarModel model, InjectionArgs args); }
```

詳しい例は [カスタムレンダラー](./custom-renderers.md) と [エディター注入](./editor-injection.md) を参照してください。

