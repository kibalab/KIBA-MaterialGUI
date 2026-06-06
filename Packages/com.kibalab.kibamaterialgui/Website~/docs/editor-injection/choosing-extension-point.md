# Choosing An Extension Point

KIBAMaterialGUI has two extension styles:

- **Raw hook injection** draws IMGUI at a named point in the inspector.
- **Contribution APIs** modify a specific model such as the toolbar, group action buttons, context menus, diagnostics, or filters.

Prefer contribution APIs when one exists. Use raw hooks for small custom UI that does not fit a model.

## Decision Guide

| You want to... | Use |
| --- | --- |
| Draw a helper box after the toolbar | `IShaderEditor` |
| Draw controls at the top of one group | `IShaderEditor` with `BeforeGroupContent` |
| Add a button to the toolbar | `IToolbarContributor` |
| Hide or lock a built-in toolbar item | `IToolbarContributor` |
| Add a tiny button in a group header | `IMaterialGUIGroupActionContributor` |
| Add group right-click commands | `IGroupMenuContributor` |
| Report shader-author problems | `IMaterialGUIDiagnosticProvider` |
| Add filter shortcuts | `IMaterialGUIFilterProvider` |
| Replace how one property field draws | Custom property renderer |

## Raw Hook vs Contribution

Raw hook:

```csharp
[ShaderEditorInjection(HookPoint.AfterToolbar)]
public sealed class MyHook : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        UnityEditor.EditorGUILayout.HelpBox("Custom UI", UnityEditor.MessageType.Info);
    }
}
```

Contribution:

```csharp
[MaterialGUIContribution(ContributionTarget.Toolbar)]
public sealed class MyButton : IToolbarContributor
{
    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Add(new ToolbarItem
        {
            Id = "my.button",
            Type = ToolbarItemType.Button,
            Label = "Run",
            OnClick = () => UnityEngine.Debug.Log("Run")
        });
    }
}
```

Use contribution APIs when you want your UI to visually blend with KIBAMaterialGUI's existing controls.

## Scope Extensions

Always scope extensions when they are shader-specific.

```csharp
[ShaderEditorInjection(
    HookPoint.BeforeGroupContent,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" },
    GroupPath = "Lighting")]
```

Without shader filters, an extension can appear on every KIBAMaterialGUI shader.

## Group Path Rule

ShaderLab uses comma-separated group declarations:

```shaderlab
[Group(Surface, Skin)] _SkinMask ("Skin Mask", 2D) = "white" {}
```

Extension filters use the resolved path:

```csharp
GroupPath = "Surface/Skin"
```

## When Not To Use Injection

Do not use injection for:

- changing how one field draws,
- large modal workflows,
- persistent project settings windows,
- unrelated global editor tools.

For field rendering, use [Custom Renderers](../custom-renderers.md). For larger workflows, create a normal Unity `EditorWindow` and link to it from a small injected button.
