# マッチングと優先順位

KIBAMaterialGUI は登録されたレンダラーから、現在のプロパティに合うものを選択します。

```csharp
[MaterialGUIPropertyRenderer(
    Order = -100,
    PropertyName = "_MainTex",
    PropertyTypes = new[] { MaterialProperty.PropType.Texture },
    RequireShaderAttributes = new[] { "GradientTexture" })]
```

## 主な条件

- `PropertyName`
- `PropertyTypes`
- `RequireShaderAttributes`
- `ExcludeShaderAttributes`
- `ShaderNameEquals`
- `ShaderNameContains`
- `ShaderNameRegex`
- `RequireProperties`
- `RequireKeywords`

## Order

`Order` が小さいほど先に選ばれます。プロジェクト専用レンダラーを標準レンダラーより優先する場合は負の値を使います。

## 動的条件

静的条件だけで足りない場合は `IMaterialGUIPropertyRendererFilter` を実装します。

```csharp
public bool CanRender(PropertyRendererArgs args)
    => args.Material != null && args.Material.HasProperty("_Mode");
```

