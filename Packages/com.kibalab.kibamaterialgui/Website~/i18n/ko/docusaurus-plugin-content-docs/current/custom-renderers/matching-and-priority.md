# 매칭과 우선순위

KIBAMaterialGUI은 등록된 렌더러 중 현재 프로퍼티에 맞는 렌더러를 선택합니다.

## 기본 매칭 조건

`MaterialGUIPropertyRendererAttribute`에서 조건을 지정합니다.

```csharp
[MaterialGUIPropertyRenderer(
    Order = -100,
    PropertyName = "_MainTex",
    PropertyTypes = new[] { MaterialProperty.PropType.Texture },
    RequireShaderAttributes = new[] { "GradientTexture" })]
```

자주 쓰는 조건은 다음과 같습니다.

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

`Order` 값이 낮을수록 먼저 선택됩니다. 프로젝트 전용 렌더러가 기본 렌더러보다 먼저 매칭되어야 한다면 음수 값을 사용하세요.

```csharp
[MaterialGUIPropertyRenderer(Order = -100)]
```

## 동적 조건

정적 Attribute 조건만으로 부족하면 `IMaterialGUIPropertyRendererFilter`를 함께 구현합니다.

```csharp
public sealed class MyRenderer :
    IMaterialGUIPropertyRenderer,
    IMaterialGUIPropertyRendererFilter
{
    public bool CanRender(PropertyRendererArgs args)
        => args.Material != null && args.Material.HasProperty("_Mode");
}
```

