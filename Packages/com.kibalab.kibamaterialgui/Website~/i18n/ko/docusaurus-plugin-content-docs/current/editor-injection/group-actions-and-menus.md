# 그룹 Action과 메뉴

그룹 확장은 그룹 헤더 버튼과 우클릭 메뉴를 추가할 때 사용합니다.

## 그룹 헤더 버튼

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

## 그룹 메뉴

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

## 권장 설계

- 자주 쓰는 명령은 그룹 헤더 버튼
- 덜 자주 쓰는 명령은 컨텍스트 메뉴
- 위험한 변경은 메뉴에서 확인 단계를 추가

