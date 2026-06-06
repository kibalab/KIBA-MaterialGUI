# 診断とフィルター

診断 provider とフィルター provider は、大きなインスペクターで問題のある値を見つけやすくします。

## 診断

```csharp
[MaterialGUIContribution(ContributionTarget.Diagnostic)]
public sealed class TextureDiagnostic : IMaterialGUIDiagnosticProvider
{
    public void Contribute(List<MaterialGUIDiagnostic> diagnostics, InjectionArgs args)
    {
        var mainTex = args.Context.Properties.FirstOrDefault(p => p.name == "_MainTex");
        if (mainTex != null && mainTex.textureValue == null)
        {
            diagnostics.Add(new MaterialGUIDiagnostic(
                MaterialGUIDiagnosticSeverity.Warning,
                "Main texture is missing.",
                "_MainTex"));
        }
    }
}
```

## フィルター

```csharp
[MaterialGUIContribution(ContributionTarget.Filter)]
public sealed class ProjectFilter : IMaterialGUIFilterProvider
{
    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Add(ToolbarItem.Toggle(
            "project.onlyWarnings",
            "Project Warnings",
            false,
            value => SetProjectWarningFilter(value)));
    }
}
```

診断は編集を止めるものではなく、問題のある値へすばやく移動するための補助情報として設計します。

