# Hook Injection

Hook injection draws IMGUI before or after known parts of the KIBAMaterialGUI inspector.

Use it for small helper UI that should appear near existing material controls.

## Basic Example

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;

[ShaderEditorInjection(
    HookPoint.AfterToolbar,
    Order = 0,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" })]
public sealed class MyShaderNotice : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        EditorGUILayout.HelpBox("Use the Lighting group first.", MessageType.Info);
    }
}
```

## Group Helper Example

This draws controls at the start of the `Lighting` group.

```csharp
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEditor;
using UnityEngine;

[ShaderEditorInjection(
    HookPoint.BeforeGroupContent,
    Order = 0,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" },
    GroupPath = "Lighting")]
public sealed class LightingQuickControls : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        var intensity = args.Context.Properties.FirstOrDefault(p => p.name == "_LightIntensity");
        if (intensity == null) return;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Soft", GUILayout.Width(70)))
                MaterialGUIPropertyChangeUtility.SetFloat(args.Context, intensity, 0.25f, "Set Soft Lighting");

            if (GUILayout.Button("Bright", GUILayout.Width(70)))
                MaterialGUIPropertyChangeUtility.SetFloat(args.Context, intensity, 1.0f, "Set Bright Lighting");
        }
    }
}
```

## Property Helper Example

This draws a small helper after one property.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;

[ShaderEditorInjection(
    HookPoint.AfterProperty,
    Order = 0,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" },
    GroupPath = "Lighting",
    PropertyName = "_LightIntensity")]
public sealed class LightIntensityHint : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        EditorGUILayout.LabelField("Recommended range: 0.25 - 1.0", EditorStyles.miniLabel);
    }
}
```

## Hook Points

| Hook point | Typical use |
| --- | --- |
| `BeforeHeader` | UI before KIBAMaterialGUI's inspector header |
| `AfterHeader` | notices or quick links after the header |
| `BeforeToolbar` | controls before the search/filter toolbar |
| `AfterToolbar` | shader-specific toolbar rows |
| `BeforeTree` | UI before grouped properties |
| `AfterTree` | UI after grouped properties |
| `BeforeGroupHeader` | compact UI before a group header |
| `AfterGroupHeader` | badges or helper text after a group header |
| `BeforeGroupContent` | helper UI at the start of a group |
| `AfterGroupContent` | helper UI at the end of a group |
| `BeforeProperty` | UI before a property row |
| `AfterProperty` | UI after a property row |
| `BeforeFooter` | UI before presets/render queue/footer controls |
| `AfterFooter` | final shader-specific UI |

## Filters

`ShaderEditorInjectionAttribute` supports:

- `Order`
- `ShaderNameEquals`
- `ShaderNameContains`
- `ShaderNameRegex`
- `RequireProperties`
- `RequireKeywords`
- `GroupPath`
- `PropertyName`

Use `Order` when multiple injections target the same hook. Lower values draw first.

## InjectionArgs

Useful fields:

- `Hook`: current hook point.
- `Context`: `MaterialGUIContext`.
- `GroupPath`: current resolved group path.
- `Property`: current property for property hooks.
- `PropertyLabel`: localized property label for property hooks.
- `Depth`: group nesting depth.

## Writing Material Values

Use `MaterialGUIPropertyChangeUtility` from injected UI.

```csharp
MaterialGUIPropertyChangeUtility.SetFloat(args.Context, property, 1.0f, "Set Value");
```

This registers undo and marks the inspector as changed.
