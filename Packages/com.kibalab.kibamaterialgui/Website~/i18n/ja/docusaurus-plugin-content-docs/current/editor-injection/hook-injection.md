# Hook Injection

Hook injection は、KIBAMaterialGUI インスペクターの特定位置の前後に IMGUI を描画します。

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

## グループ用 UI

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

値を書き込む場合は `MaterialGUIPropertyChangeUtility` を使います。

