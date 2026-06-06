# Scripting API

This page summarizes the public C# surface intended for shader package authors and editor extension authors.

All APIs are Editor-only.

For task-oriented guides, start with:

- [Custom Renderers](./custom-renderers.md)
- [Editor Injection](./editor-injection.md)

## Namespaces

| Namespace | Purpose |
| --- | --- |
| `KIBA_.KIBAMaterialGUI.Editor.Core` | context, model, diagnostics |
| `KIBA_.KIBAMaterialGUI.Editor.Extensibility` | editor injection and contribution APIs |
| `KIBA_.KIBAMaterialGUI.Editor.ShaderLab` | ShaderLab drawer bridge classes |
| `KIBA_.KIBAMaterialGUI.Editor.UI.Property` | property renderer APIs and value write helpers |

## Core Context

### `MaterialGUIContext`

Read-only context passed to extension callbacks.

| Member | Type | Description |
| --- | --- | --- |
| `MaterialEditor` | `MaterialEditor` | Unity material editor instance. |
| `Material` | `Material` | Primary selected material. |
| `Targets` | `IReadOnlyList<Material>` | Selected materials. |
| `Properties` | `IReadOnlyList<MaterialProperty>` | Material properties for the current inspector. |
| `CurrentLanguage` | `string` | Active localization language code. |
| `Search` | `string` | Current search text. |
| `Model` | `MaterialGUIModel` | Current grouped property model. |

### `MaterialGUIModel`

| Member | Type | Description |
| --- | --- | --- |
| `Root` | `GroupNodeModel` | Root group node. |
| `Properties` | `IReadOnlyList<ShaderPropertyModel>` | All property models. |
| `Diagnostics` | `IReadOnlyList<MaterialGUIDiagnostic>` | Inspector diagnostics. |

### `ShaderPropertyModel`

| Member | Type | Description |
| --- | --- | --- |
| `Property` | `MaterialProperty` | Unity material property. |
| `PropertyName` | `string` | Shader property name. |
| `Label` | `string` | Original display label. |
| `TranslatedLabel` | `string` | Localized label. |
| `GroupPath` | `string` | Resolved group path. |
| `PropertyType` | `MaterialProperty.PropType` | Unity property type. |
| `Attributes` | `IReadOnlyList<ShaderAttributeInfo>` | ShaderLab attributes. |
| `Changed` | `bool` | Value differs from shader default. |
| `Mixed` | `bool` | Multi-selection mixed value state. |
| `ConditionVisible` | `bool` | Result of `[ShowIf]` visibility. |
| `SearchMatched` | `bool` | Result of current search. |
| `Visible` | `bool` | Final row visibility. |
| `ConditionalVisibility` | `ConditionalVisibilityInfo` | Parsed condition state. |
| `Diagnostics` | `IReadOnlyList<MaterialGUIDiagnostic>` | Property diagnostics. |
| `WarningCount` | `int` | Warning/error diagnostic count. |

### `GroupNodeModel`

| Member | Type | Description |
| --- | --- | --- |
| `Name` | `string` | Group segment name. |
| `PathKey` | `string` | Full group path. |
| `Expanded` | `bool` | Foldout state. |
| `Visible` | `bool` | Final group visibility. |
| `SearchMatchedSelf` | `bool` | Whether the group path matched search. |
| `Properties` | `IReadOnlyList<ShaderPropertyModel>` | Direct properties in the group. |
| `Children` | `IReadOnlyDictionary<string, GroupNodeModel>` | Child groups. |
| `TotalPropertyCount` | `int` | Total visible-aware property count. |
| `VisiblePropertyCount` | `int` | Currently visible property count. |
| `ChangedCount` | `int` | Changed property count. |
| `WarningCount` | `int` | Warning/error count. |
| `HasMixed` | `bool` | Whether the group contains mixed values. |

### `MaterialGUIDiagnostic`

```csharp
public MaterialGUIDiagnostic(
    MaterialGUIDiagnosticSeverity severity,
    string message,
    string propertyName = null,
    string groupPath = null)
```

Use diagnostics for shader-author feedback. Severity values are `Info`, `Warning`, and `Error`.

## Property Renderer API

See also:

- [Your First Renderer](./custom-renderers/first-renderer.md)
- [Matching And Priority](./custom-renderers/matching-and-priority.md)
- [Typed Attribute Renderers](./custom-renderers/typed-attribute-renderers.md)

### `IMaterialGUIPropertyRenderer`

```csharp
public interface IMaterialGUIPropertyRenderer
{
    float GetHeight(PropertyRendererArgs args);
    Rect OnGUI(PropertyRendererArgs args);
}
```

### `IMaterialGUIPropertyRendererFilter`

```csharp
public interface IMaterialGUIPropertyRendererFilter
{
    bool CanRender(PropertyRendererArgs args);
}
```

Implement this when static attribute filters are not enough.

### `MaterialGUIPropertyRendererAttribute`

| Property | Type | Description |
| --- | --- | --- |
| `Order` | `int` | Lower values resolve first. |
| `ShaderNameEquals` | `string[]` | Exact shader name filters. |
| `ShaderNameContains` | `string[]` | Substring shader name filters. |
| `ShaderNameRegex` | `string` | Regex shader name filter. |
| `RequireProperties` | `string[]` | Required material properties. |
| `RequireKeywords` | `string[]` | Required enabled keywords. |
| `PropertyName` | `string` | Required property name. |
| `PropertyTypes` | `MaterialProperty.PropType[]` | Required property types. |
| `RequireShaderAttributes` | `string[]` | Required ShaderLab attributes. |
| `ExcludeShaderAttributes` | `string[]` | Disallowed ShaderLab attributes. |

### `PropertyRendererArgs`

| Member | Type | Description |
| --- | --- | --- |
| `Position` | `Rect` | Full row rect passed to the renderer. |
| `Layout` | `PropertyRowLayout` | Detailed row layout. |
| `MaterialEditor` | `MaterialEditor` | Unity material editor. |
| `Material` | `Material` | Primary material. |
| `Property` | `MaterialProperty` | Current property. |
| `Label` | `string` | Localized label. |
| `MiniGray` | `GUIStyle` | Small gray label style. |
| `Shader` | `Shader` | Resolved shader. |
| `Model` | `ShaderPropertyModel` | Optional property model. |
| `LabelRect` | `Rect` | Label rect. |
| `FieldRect` | `Rect` | Field rect. |
| `ResetRect` | `Rect` | Reset button rect. |

Methods:

- `HasShaderAttribute(string attributeName)`
- `TryGetShaderAttribute(string attributeName, out ShaderAttributeInfo attribute)`
- `GetShaderAttributes()`
- `SetFloatValue(float value, string undoName = null)`
- `SetColorValue(Color value, string undoName = null)`
- `SetVectorValue(Vector4 value, string undoName = null)`
- `SetTextureValue(Texture value, string undoName = null)`
- `SetTextureScaleAndOffset(Vector4 value, string undoName = null)`
- `RegisterPropertyValueChange(string undoName = null)`

### `ShaderPropertyRenderer<TArgs>`

Convenience base class for one-attribute renderers.

```csharp
public abstract class ShaderPropertyRenderer<TArgs> :
    IMaterialGUIPropertyRenderer,
    IMaterialGUIPropertyRendererFilter
```

Override:

- `AttributeName`
- `SupportsPropertyType`
- `TryParseArguments`
- `GetHeight`
- `OnGUI`

There are also two-, three-, and four-argument base classes:

- `ShaderPropertyRenderer<T1, T2>`
- `ShaderPropertyRenderer<T1, T2, T3>`
- `ShaderPropertyRenderer<T1, T2, T3, T4>`

## ShaderLab Bridge API

### `ShaderPropertyDrawer`

Base class for ShaderLab attribute bridge drawers.

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class MyHintDrawer : ShaderPropertyDrawer
{
    public MyHintDrawer() {}
}
```

Create a drawer class named `{AttributeName}Drawer` so Unity accepts `[AttributeName]` in shader properties.

## Injection API

See also:

- [Choosing An Extension Point](./editor-injection/choosing-extension-point.md)
- [Hook Injection](./editor-injection/hook-injection.md)

### `IShaderEditor`

```csharp
public interface IShaderEditor
{
    void OnGUI(InjectionArgs args);
}
```

### `ShaderEditorInjectionAttribute`

| Property | Type | Description |
| --- | --- | --- |
| `Hook` | `HookPoint` | Required constructor argument. |
| `Order` | `int` | Lower values draw first. |
| `ShaderNameEquals` | `string[]` | Exact shader filters. |
| `ShaderNameContains` | `string[]` | Substring shader filters. |
| `ShaderNameRegex` | `string` | Regex shader filter. |
| `RequireProperties` | `string[]` | Required material properties. |
| `RequireKeywords` | `string[]` | Required enabled keywords. |
| `GroupPath` | `string` | Required group path. |
| `PropertyName` | `string` | Required property name. |

### `HookPoint`

Values:

- `BeforeHeader`
- `AfterHeader`
- `BeforeToolbar`
- `AfterToolbar`
- `BeforeTree`
- `AfterTree`
- `BeforeGroupHeader`
- `AfterGroupHeader`
- `BeforeGroupContent`
- `AfterGroupContent`
- `BeforeProperty`
- `AfterProperty`
- `BeforeFooter`
- `AfterFooter`

### `InjectionArgs`

| Member | Type | Description |
| --- | --- | --- |
| `Hook` | `HookPoint` | Current hook point. |
| `Context` | `MaterialGUIContext` | Inspector context. |
| `GroupPath` | `string` | Current group path when available. |
| `Property` | `MaterialProperty` | Current property when available. |
| `PropertyLabel` | `string` | Current localized label when available. |
| `Depth` | `int` | Group depth for group hooks. |

## Contribution API

See also:

- [Toolbar Contributions](./editor-injection/toolbar-contributions.md)
- [Group Actions And Menus](./editor-injection/group-actions-and-menus.md)
- [Diagnostics And Filters](./editor-injection/diagnostics-and-filters.md)

### `MaterialGUIContributionAttribute`

```csharp
[MaterialGUIContribution(ContributionTarget.Toolbar, Order = 10)]
```

Filters:

- `Order`
- `ShaderNameEquals`
- `ShaderNameContains`
- `ShaderNameRegex`
- `GroupPath`

Targets:

- `Toolbar`
- `GroupContextMenu`
- `GroupAction`
- `Diagnostic`
- `Filter`

### Contributor Interfaces

```csharp
public interface IToolbarContributor
{
    void Contribute(ToolbarModel model, InjectionArgs args);
}

public interface IGroupMenuContributor
{
    void Contribute(ContextMenuModel model, InjectionArgs args);
}

public interface IMaterialGUIGroupActionContributor
{
    void Contribute(GroupActionModel model, InjectionArgs args);
}

public interface IMaterialGUIDiagnosticProvider
{
    void Contribute(List<MaterialGUIDiagnostic> diagnostics, InjectionArgs args);
}

public interface IMaterialGUIFilterProvider
{
    void Contribute(ToolbarModel model, InjectionArgs args);
}
```

## Model APIs

### `ToolbarModel`

Methods:

- `Add(ToolbarItem item)`
- `Remove(string id)`
- `Find(string id)`
- `Hide(string id)`
- `Lock(string id, bool locked = true)`

`ToolbarItem` supports:

- `Toggle`
- `Button`
- `Input`
- `Dropdown`

### `GroupActionModel`

Methods:

- `Add(GroupActionItem item)`
- `Remove(string id)`
- `Find(string id)`

Properties:

- `Items`
- `BeginDrag`

### `ContextMenuModel`

Methods:

- `Add(ContextMenuItem item)`
- `Remove(string id)`
- `Find(string id)`
- `Hide(string id)`
- `Lock(string id, bool locked = true)`

`ContextMenuItemType` values:

- `Toggle`
- `Button`
- `Separator`

## Value Write Utility

Use `MaterialGUIPropertyChangeUtility` outside property renderers.

```csharp
MaterialGUIPropertyChangeUtility.SetFloat(context, property, value, "Change Float");
MaterialGUIPropertyChangeUtility.SetColor(context, property, value, "Change Color");
MaterialGUIPropertyChangeUtility.SetVector(context, property, value, "Change Vector");
MaterialGUIPropertyChangeUtility.SetTexture(context, property, value, "Change Texture");
MaterialGUIPropertyChangeUtility.SetTextureScaleAndOffset(context, property, value, "Change Texture Scale");
MaterialGUIPropertyChangeUtility.RegisterChange(context, property, "Change Material Property");
```

These methods register Unity undo through `MaterialEditor`, set `GUI.changed`, and then write the material property value.
