# 진단과 필터

진단 provider와 필터 provider는 쉐이더 작성자가 문제를 발견하고 큰 인스펙터를 빠르게 좁히도록 돕습니다.

## 진단 추가

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

## 필터 추가

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

진단은 사용자의 편집 흐름을 막기보다, 문제가 있는 값을 빠르게 찾게 하는 보조 정보로 설계하는 것이 좋습니다.

