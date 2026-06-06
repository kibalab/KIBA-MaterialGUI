# Diagnostics And Filters

Diagnostics and filter contributions help shader authors understand and navigate large inspectors.

Use diagnostics for feedback. Use filter contributions for search/filter shortcuts.

## Add A Diagnostic

```csharp
using System.Collections.Generic;
using KIBA_.KIBAMaterialGUI.Editor.Core;
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;

[MaterialGUIContribution(
    ContributionTarget.Diagnostic,
    Order = 10,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" })]
public sealed class EmissionDiagnostic : IMaterialGUIDiagnosticProvider
{
    public void Contribute(List<MaterialGUIDiagnostic> diagnostics, InjectionArgs args)
    {
        if (!args.Context.Material.IsKeywordEnabled("_EMISSION"))
        {
            diagnostics.Add(new MaterialGUIDiagnostic(
                MaterialGUIDiagnosticSeverity.Info,
                "Emission keyword is disabled.",
                propertyName: "_EmissionColor",
                groupPath: "Lighting"));
        }
    }
}
```

## Severity

Use:

- `Info` for tips and non-blocking guidance,
- `Warning` for likely shader-author mistakes,
- `Error` for invalid setup that prevents the inspector feature from working.

Avoid reporting user preference or artistic choices as warnings.

## Property And Group Links

Set `propertyName` and `groupPath` when the diagnostic belongs to a specific place.

```csharp
new MaterialGUIDiagnostic(
    MaterialGUIDiagnosticSeverity.Warning,
    "The rim color is visible only when _UseRim is enabled.",
    propertyName: "_RimColor",
    groupPath: "Lighting/Rim");
```

## Filter Shortcut

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;

[MaterialGUIContribution(
    ContributionTarget.Filter,
    Order = 20,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" })]
public sealed class LightingSearchShortcut : IMaterialGUIFilterProvider
{
    public void Contribute(ToolbarModel model, InjectionArgs args)
    {
        model.Add(new ToolbarItem
        {
            Id = "filter.lighting",
            Type = ToolbarItemType.Button,
            Label = "Lighting",
            Width = 78f,
            Tooltip = "Search lighting properties.",
            OnClick = () => model.Find("search")?.TextSetter?.Invoke("Lighting")
        });
    }
}
```

## Filter Controls

Filter contributors receive the same `ToolbarModel` shape as toolbar contributors. You can add:

- buttons,
- toggles,
- inputs,
- dropdowns.

Use distinct IDs and keep controls compact.

## Practical Rules

- Diagnostics should help shader authors fix something.
- Keep messages short and specific.
- Scope diagnostics by shader when they are shader-specific.
- Prefer `Info` unless the issue is likely a real mistake.
- Do not use filter contributions to hide properties directly; update search/filter state or add shortcut controls instead.
