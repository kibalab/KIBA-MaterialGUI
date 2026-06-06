# ShaderLab Bridge

ShaderLab bridge は、Unity がカスタム Attribute をパースできるようにする最小 drawer です。

```csharp
using KIBA_.KIBAMaterialGUI.Editor.ShaderLab;

public sealed class MyHintDrawer : ShaderPropertyDrawer
{
    public MyHintDrawer() {}
}
```

この例では `[MyHint]` をシェーダーで使えるようになります。

## 名前規則

| ShaderLab | C# drawer |
| --- | --- |
| `[StepFloat]` | `StepFloatDrawer` |
| `[MyHint]` | `MyHintDrawer` |

Attribute 引数は KIBAMaterialGUI の `ShaderAttributeInfo` から読み取ります。Bridge drawer は通常、空のコンストラクターだけで十分です。

