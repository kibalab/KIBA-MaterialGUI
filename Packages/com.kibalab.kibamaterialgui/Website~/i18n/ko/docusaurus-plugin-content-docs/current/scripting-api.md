# Scripting API

이 페이지는 쉐이더 패키지 작성자가 주로 사용하는 공개 C# API를 요약합니다. 모든 API는 Editor 전용입니다.

## 주요 네임스페이스

| 네임스페이스 | 용도 |
| --- | --- |
| `KIBA_.KIBAMaterialGUI.Editor.Core` | 모델, 컨텍스트, 진단 |
| `KIBA_.KIBAMaterialGUI.Editor.Extensibility` | 인젝션과 contribution API |
| `KIBA_.KIBAMaterialGUI.Editor.ShaderLab` | ShaderLab drawer bridge |
| `KIBA_.KIBAMaterialGUI.Editor.UI.Property` | 프로퍼티 렌더러와 값 쓰기 helper |

## Context

`MaterialGUIContext`는 확장 콜백에 전달되는 읽기 중심 컨텍스트입니다.

| 멤버 | 설명 |
| --- | --- |
| `MaterialEditor` | 현재 Unity `MaterialEditor` |
| `Material` | 대표 선택 머티리얼 |
| `Targets` | 선택된 모든 머티리얼 |
| `Properties` | 현재 머티리얼 프로퍼티 |
| `CurrentLanguage` | 현재 표시 언어 |
| `Search` | 현재 검색어 |
| `Model` | 그룹/프로퍼티 모델 |

## 프로퍼티 렌더러

```csharp
public interface IMaterialGUIPropertyRenderer
{
    float GetHeight(PropertyRendererArgs args);
    Rect OnGUI(PropertyRendererArgs args);
}
```

`MaterialGUIPropertyRendererAttribute`로 매칭 조건을 지정합니다.

```csharp
[MaterialGUIPropertyRenderer(
    Order = -100,
    PropertyTypes = new[] { MaterialProperty.PropType.Float },
    RequireShaderAttributes = new[] { "StepFloat" })]
```

값 쓰기는 반드시 helper를 사용하세요.

```csharp
args.SetFloatValue(value);
args.SetColorValue(color);
args.SetVectorValue(vector);
args.SetTextureValue(texture);
args.RegisterPropertyValueChange();
```

## 에디터 인젝션

```csharp
public interface IShaderEditor
{
    void OnGUI(InjectionArgs args);
}
```

```csharp
[ShaderEditorInjection(HookPoint.AfterToolbar, Order = 0)]
public sealed class MyInjection : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        EditorGUILayout.LabelField("Project helper");
    }
}
```

주요 HookPoint는 `AfterToolbar`, `BeforeGroupContent`, `AfterGroupContent`, `BeforeProperty`, `AfterProperty`, `AfterFooter`입니다.

## Contribution API

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

## 모델

`ShaderPropertyModel`은 프로퍼티명, 라벨, 그룹 경로, Attribute 목록, 타입, 변경 여부, mixed 상태, `[ShowIf]` 결과, 진단을 포함합니다.

`GroupNodeModel`은 그룹 경로, 펼침 상태, 표시 여부, 자식 그룹, 직접 프로퍼티, 경고 수, mixed 상태를 포함합니다.

자세한 사용 예시는 [커스텀 렌더러](./custom-renderers.md)와 [에디터 인젝션](./editor-injection.md)을 참고하세요.

