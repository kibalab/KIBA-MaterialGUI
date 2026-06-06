# 툴바 Contribution

툴바 contributor는 검색/필터 영역 근처에 프로젝트 전용 컨트롤을 추가할 때 사용합니다.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;

[MaterialGUIContribution(ContributionTarget.Toolbar, Order = 10)]
public sealed class MyToolbarButton : IToolbarContributor
{
    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Add(ToolbarItem.Button(
            "my.resetLighting",
            "Reset Lighting",
            () => ResetLighting(args.Context)));
    }
}
```

## 사용 기준

- 현재 머티리얼 전체에 영향을 주는 명령
- 검색/필터와 함께 쓰는 도구
- 프로젝트 공통 워크플로 버튼

특정 그룹에만 필요한 명령은 그룹 action이나 그룹 메뉴가 더 적합합니다.

