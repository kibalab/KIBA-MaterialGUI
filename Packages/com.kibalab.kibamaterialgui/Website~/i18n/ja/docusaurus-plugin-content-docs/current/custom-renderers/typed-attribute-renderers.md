# 型付き Attribute レンダラー

1つの ShaderLab Attribute をパースして描画する場合は、`ShaderPropertyRenderer<TArgs>` 系の base class が便利です。

## 使う場面

- Attribute 引数のパースをレンダラー内にまとめたい
- 同じ Attribute を複数プロパティで使いたい
- 不正な引数を一貫して扱いたい

## 例

```csharp
public sealed class UnitRenderer : ShaderPropertyRenderer<string>
{
    protected override string AttributeName => "Unit";

    protected override bool SupportsPropertyType(MaterialProperty.PropType type)
        => type == MaterialProperty.PropType.Float || type == MaterialProperty.PropType.Range;

    protected override bool TryParseArguments(ShaderAttributeInfo attribute, out string unit)
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

複数引数には `ShaderPropertyRenderer<T1, T2>`、`ShaderPropertyRenderer<T1, T2, T3>`、`ShaderPropertyRenderer<T1, T2, T3, T4>` を使えます。

