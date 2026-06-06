# Value Writes And Context

Custom renderers are responsible for drawing one property field. KIBAMaterialGUI still handles group UI, reset buttons, animation scopes, diagnostics, and validation around that row.

## Use PropertyRendererArgs Helpers

Do this:

```csharp
if (EditorGUI.EndChangeCheck())
    args.SetFloatValue(next, "Change Intensity");
```

Avoid this:

```csharp
args.Property.floatValue = next;
```

Direct writes can bypass Unity undo and animation recording paths.

## Write Methods

`PropertyRendererArgs` provides:

- `SetFloatValue(float value, string undoName = null)`
- `SetColorValue(Color value, string undoName = null)`
- `SetVectorValue(Vector4 value, string undoName = null)`
- `SetTextureValue(Texture value, string undoName = null)`
- `SetTextureScaleAndOffset(Vector4 value, string undoName = null)`
- `RegisterPropertyValueChange(string undoName = null)`

Use `RegisterPropertyValueChange` only when you need to perform a custom write that KIBAMaterialGUI does not wrap directly.

## Reading Row State

Useful members:

| Member | Meaning |
| --- | --- |
| `Position` | Rect reserved for the renderer. |
| `Layout` | Full row layout with label, field, reset, and main rects. |
| `MaterialEditor` | Unity `MaterialEditor`. |
| `Material` | Primary selected material. |
| `Property` | Current `MaterialProperty`. |
| `Label` | Localized display label. |
| `Shader` | Resolved shader. |
| `Model` | Optional `ShaderPropertyModel`. |

## Using Layout Rects

Use `FieldRect` when you want Unity-style label/field separation.

```csharp
EditorGUI.LabelField(args.LabelRect, args.Label);

EditorGUI.BeginChangeCheck();
var next = EditorGUI.Slider(args.FieldRect, GUIContent.none, args.Property.floatValue, 0f, 1f);
if (EditorGUI.EndChangeCheck())
    args.SetFloatValue(next);
```

Use `Position` when drawing a standard IMGUI field that includes its own label.

```csharp
var next = EditorGUI.FloatField(args.Position, args.Label, args.Property.floatValue);
```

## Reading Shader Attributes

```csharp
if (args.HasShaderAttribute("MyHint"))
{
    // Attribute exists.
}

if (args.TryGetShaderAttribute("MyHint", out var attribute))
{
    Debug.Log(attribute.Args);
}

foreach (var attribute in args.GetShaderAttributes())
{
    Debug.Log(attribute.Name);
}
```

## Using The Property Model

`args.Model` is optional, but when present it gives the current KIBAMaterialGUI model state.

```csharp
if (args.Model != null && args.Model.Mixed)
{
    EditorGUI.showMixedValue = true;
}
```

Common fields:

- `GroupPath`
- `Changed`
- `Mixed`
- `ConditionVisible`
- `SearchMatched`
- `Visible`
- `Diagnostics`
- `WarningCount`

## Multi-Selection

`MaterialProperty` already represents Unity's multi-selection property wrapper. Write through `PropertyRendererArgs` helpers and Unity applies the change to selected materials consistently.

Do not loop over `args.Property.targets` unless you are doing something that cannot be represented through `MaterialProperty`.

## Animation Recording

KIBAMaterialGUI wraps property rows with Unity material property and animation scopes. Custom renderers inherit that behavior as long as they write through `PropertyRendererArgs` helpers.

Keep custom UI focused on the field itself. Reset buttons and context menus should remain outside your renderer.
