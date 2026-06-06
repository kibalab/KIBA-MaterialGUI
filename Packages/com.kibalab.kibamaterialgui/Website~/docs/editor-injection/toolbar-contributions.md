# Toolbar Contributions

Toolbar contributions add, hide, or lock controls in KIBAMaterialGUI's toolbar model.

Use them when you want controls to align with KIBAMaterialGUI's built-in toolbar instead of drawing a separate IMGUI row.

## Add A Button

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;

[MaterialGUIContribution(
    ContributionTarget.Toolbar,
    Order = 20,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" })]
public sealed class CopyMaterialNameButton : IToolbarContributor
{
    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Add(new ToolbarItem
        {
            Id = "my.copy-material-name",
            Type = ToolbarItemType.Button,
            Label = "Copy Name",
            Width = 90f,
            Tooltip = "Copy the current material name.",
            OnClick = () => EditorGUIUtility.systemCopyBuffer = args.Context.Material.name
        });
    }
}
```

## Add A Toggle

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;

[MaterialGUIContribution(ContributionTarget.Toolbar, Order = 30)]
public sealed class DebugToggle : IToolbarContributor
{
    private const string Key = "MyShader.DebugToolbar";

    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Add(new ToolbarItem
        {
            Id = "my.debug-toggle",
            Type = ToolbarItemType.Toggle,
            Label = "Debug",
            Width = 70f,
            ToggleGetter = () => EditorPrefs.GetBool(Key, false),
            ToggleSetter = value => EditorPrefs.SetBool(Key, value)
        });
    }
}
```

## Add A Dropdown

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEngine;

[MaterialGUIContribution(ContributionTarget.Toolbar, Order = 40)]
public sealed class QualityDropdown : IToolbarContributor
{
    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Add(new ToolbarItem
        {
            Id = "my.quality",
            Type = ToolbarItemType.Dropdown,
            Label = "Quality",
            Width = 90f,
            DropdownLabelGetter = () => "Quality",
            Options =
            {
                new DropdownOption { Id = "low", Label = "Low", OnSelect = () => Debug.Log("Low") },
                new DropdownOption { Id = "high", Label = "High", OnSelect = () => Debug.Log("High") }
            }
        });
    }
}
```

## Hide Or Lock Built-In Items

```csharp
[MaterialGUIContribution(
    ContributionTarget.Toolbar,
    Order = 5,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" })]
public sealed class ToolbarPolicy : IToolbarContributor
{
    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Hide("presets");
        model.Lock("double_sided_toggle", true);
    }
}
```

Built-in item IDs are intentionally small and stable where exposed, but this area is still preview API. Keep toolbar policy code scoped to shaders you own.

## ToolbarItem Types

| Type | Use |
| --- | --- |
| `Button` | One-shot action. |
| `Toggle` | Boolean state. |
| `Input` | Text, int, or float input. |
| `Dropdown` | Menu of options. |

## Practical Rules

- Use unique `Id` values.
- Keep labels short.
- Set `Width` so the toolbar remains stable.
- Use `Order` to place items consistently.
- Scope shader-specific toolbar controls with shader filters.
