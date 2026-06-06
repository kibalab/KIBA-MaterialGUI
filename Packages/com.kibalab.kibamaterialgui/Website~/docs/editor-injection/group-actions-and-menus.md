# Group Actions And Menus

Group extensions add controls to group headers.

Use group actions for compact always-visible buttons. Use group context menus for less common actions.

## Group Header Action

```csharp
using System.Linq;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using UnityEngine;

[MaterialGUIContribution(
    ContributionTarget.GroupAction,
    Order = 10,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" },
    GroupPath = "Lighting")]
public sealed class SoftLightingAction : IMaterialGUIGroupActionContributor
{
    public void Contribute(GroupActionModel model, InjectionArgs args)
    {
        model.Add(new GroupActionItem
        {
            Id = "lighting.soft",
            Content = new GUIContent("S", "Apply soft lighting"),
            Tooltip = "Apply soft lighting",
            OnClick = () =>
            {
                var intensity = args.Context.Properties.FirstOrDefault(p => p.name == "_LightIntensity");
                if (intensity != null)
                    MaterialGUIPropertyChangeUtility.SetFloat(args.Context, intensity, 0.25f, "Set Soft Lighting");
            }
        });
    }
}
```

## Custom Drawn Action

Use `OnGUI` when a button needs custom drawing.

```csharp
model.Add(new GroupActionItem
{
    Id = "lighting.custom",
    Tooltip = "Custom action",
    OnGUI = rect =>
    {
        if (GUI.Button(rect, new GUIContent("!", "Custom action")))
            Debug.Log("Clicked");
    }
});
```

## Drag Source

`GroupActionModel.BeginDrag` can start a custom drag operation for a group.

```csharp
public void Contribute(GroupActionModel model, InjectionArgs args)
{
    model.BeginDrag = () =>
    {
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.SetGenericData("my.group-path", args.GroupPath);
        DragAndDrop.StartDrag(args.GroupPath ?? "Group");
    };
}
```

## Group Context Menu

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;

[MaterialGUIContribution(
    ContributionTarget.GroupContextMenu,
    Order = 10,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" },
    GroupPath = "Lighting")]
public sealed class LightingMenu : IGroupMenuContributor
{
    public void Contribute(ContextMenuModel model, InjectionArgs args)
    {
        model.Add(new ContextMenuItem
        {
            Id = "lighting.pin",
            Type = ContextMenuItemType.Toggle,
            Label = "Pin Lighting Group",
            CheckedGetter = () => EditorPrefs.GetBool("my.lighting.pin", false),
            OnClickOrToggle = () =>
            {
                var current = EditorPrefs.GetBool("my.lighting.pin", false);
                EditorPrefs.SetBool("my.lighting.pin", !current);
            }
        });
    }
}
```

## Add A Separator

```csharp
model.Add(new ContextMenuItem
{
    Id = "lighting.separator",
    Type = ContextMenuItemType.Separator,
    Order = 50
});
```

## Hide Or Lock Menu Items

```csharp
model.Hide("group.copyInternal");
model.Lock("group.reset", true);
```

Use this sparingly. Hiding built-in behavior can confuse users unless the shader package clearly replaces it.

## Choosing Action vs Menu

Use a group action for:

- frequent commands,
- one-click presets,
- small icon buttons,
- drag handles.

Use a context menu item for:

- destructive or rare commands,
- toggles,
- commands with longer labels,
- organization-specific workflow actions.
