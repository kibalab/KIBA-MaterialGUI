# Hook Injection

Hook injection은 KIBAMaterialGUI 인스펙터의 특정 지점 앞/뒤에 IMGUI를 그립니다.

## 기본 예시

```csharp
using KIBA_.KIBAMaterialGUI.Editor.Extensibility;
using UnityEditor;

[ShaderEditorInjection(
    HookPoint.AfterToolbar,
    Order = 0,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" })]
public sealed class MyShaderNotice : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        EditorGUILayout.HelpBox("Lighting group first.", MessageType.Info);
    }
}
```

## 그룹에 버튼 추가

```csharp
[ShaderEditorInjection(
    HookPoint.BeforeGroupContent,
    ShaderNameEquals = new[] { "KIBA_/Examples/MyShader" },
    GroupPath = "Lighting")]
public sealed class LightingQuickControls : IShaderEditor
{
    public void OnGUI(InjectionArgs args)
    {
        var property = args.Context.Properties.FirstOrDefault(p => p.name == "_LightIntensity");
        if (property == null) return;

        if (GUILayout.Button("Soft"))
            MaterialGUIPropertyChangeUtility.SetFloat(args.Context, property, 0.25f, "Set Soft Lighting");
    }
}
```

## 주요 HookPoint

- `AfterToolbar`: 툴바 아래 보조 UI
- `BeforeGroupContent`: 그룹 내용 시작 전
- `AfterGroupContent`: 그룹 내용 끝
- `AfterProperty`: 특정 프로퍼티 뒤 도움말
- `AfterFooter`: 인스펙터 마지막 UI

값을 쓸 때는 `MaterialGUIPropertyChangeUtility`를 사용하세요.

