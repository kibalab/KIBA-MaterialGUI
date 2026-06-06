# 타입 지정 Attribute 렌더러

하나의 ShaderLab Attribute를 파싱해서 렌더러를 만들 때는 `ShaderPropertyRenderer<TArgs>` 계열 base class를 사용할 수 있습니다.

## 사용 목적

- Attribute 인자 파싱을 렌더러 안에 모으고 싶을 때
- 같은 Attribute를 여러 프로퍼티에서 반복해서 사용할 때
- 잘못된 인자에 대한 진단을 일관되게 만들고 싶을 때

## 예시

```csharp
public sealed class UnitRenderer : ShaderPropertyRenderer<string>
{
    protected override string AttributeName => "Unit";

    protected override bool SupportsPropertyType(MaterialProperty.PropType type)
        => type == MaterialProperty.PropType.Float || type == MaterialProperty.PropType.Range;

    protected override bool TryParseArguments(
        ShaderAttributeInfo attribute,
        out string unit)
    {
        unit = attribute.Arguments.Count > 0 ? attribute.Arguments[0] : string.Empty;
        return !string.IsNullOrEmpty(unit);
    }

    protected override Rect OnGUI(PropertyRendererArgs args, string unit)
    {
        EditorGUI.BeginChangeCheck();
        var value = EditorGUI.FloatField(args.FieldRect, args.Label, args.Property.floatValue);
        GUI.Label(args.ResetRect, unit, EditorStyles.miniLabel);
        if (EditorGUI.EndChangeCheck())
            args.SetFloatValue(value);

        return args.Position;
    }
}
```

## 여러 인자

두 개 이상 인자가 필요하면 다음 base class를 사용할 수 있습니다.

- `ShaderPropertyRenderer<T1, T2>`
- `ShaderPropertyRenderer<T1, T2, T3>`
- `ShaderPropertyRenderer<T1, T2, T3, T4>`

