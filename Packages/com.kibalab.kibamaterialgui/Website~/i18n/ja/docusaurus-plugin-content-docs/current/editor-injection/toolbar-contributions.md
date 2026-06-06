# ツールバー Contribution

ツールバー contributor は検索/フィルター付近にプロジェクト専用のコントロールを追加します。

```csharp
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

マテリアル全体に影響する命令や、検索/フィルターと一緒に使う道具に向いています。

