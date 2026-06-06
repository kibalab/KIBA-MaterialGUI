# グループ Action とメニュー

グループ拡張は、グループヘッダーボタンや右クリックメニューを追加するときに使います。

## ヘッダーボタン

```csharp
[MaterialGUIContribution(ContributionTarget.GroupAction, GroupPath = "Lighting")]
public sealed class LightingAction : IMaterialGUIGroupActionContributor
{
    public void Contribute(GroupActionModel model, InjectionArgs args)
    {
        model.Add(GroupActionItem.Button(
            "lighting.reset",
            "Reset",
            () => ResetLighting(args.Context)));
    }
}
```

## メニュー

```csharp
[MaterialGUIContribution(ContributionTarget.GroupContextMenu, GroupPath = "Lighting")]
public sealed class LightingMenu : IGroupMenuContributor
{
    public void Contribute(ContextMenuModel model, InjectionArgs args)
    {
        model.Add(ContextMenuItem.Button(
            "lighting.copy",
            "Copy Lighting Values",
            () => CopyLighting(args.Context)));
    }
}
```

頻繁に使う操作はヘッダー、低頻度の操作はメニューに置くと扱いやすくなります。

