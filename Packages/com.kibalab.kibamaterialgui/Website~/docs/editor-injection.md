# Editor Injection

Editor injection adds small pieces of UI around the KIBAMaterialGUI inspector.

Use it when you want to keep KIBAMaterialGUI's normal property rendering but add shader-specific tools, shortcuts, diagnostics, or menu items.

## What It Is For

Editor injection is good for:

- buttons above or below a group,
- shader-specific toolbar actions,
- quick presets beside a group header,
- right-click group menu items,
- custom diagnostics,
- filter shortcuts,
- small helper blocks before or after a property.

Use [Custom Renderers](./custom-renderers.md) instead when the field itself needs a different editor.

## Choose The Right Extension Point

| Goal | Use |
| --- | --- |
| Draw free-form IMGUI before or after existing UI | `IShaderEditor` with `[ShaderEditorInjection]` |
| Add, hide, or lock toolbar items | `IToolbarContributor` |
| Add compact buttons to group headers | `IMaterialGUIGroupActionContributor` |
| Add right-click group menu items | `IGroupMenuContributor` |
| Add warnings/errors/info messages | `IMaterialGUIDiagnosticProvider` |
| Add filter shortcuts or custom filter controls | `IMaterialGUIFilterProvider` |

## Learning Path

1. [Choosing An Extension Point](./editor-injection/choosing-extension-point.md)
2. [Hook Injection](./editor-injection/hook-injection.md)
3. [Toolbar Contributions](./editor-injection/toolbar-contributions.md)
4. [Group Actions And Menus](./editor-injection/group-actions-and-menus.md)
5. [Diagnostics And Filters](./editor-injection/diagnostics-and-filters.md)

## Minimal Hook Example

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
        EditorGUILayout.HelpBox("This shader has project-specific helper tools.", MessageType.Info);
    }
}
```

This draws an info box after the KIBAMaterialGUI toolbar for one shader.
