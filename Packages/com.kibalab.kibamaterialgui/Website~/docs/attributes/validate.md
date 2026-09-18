# Validate

![Validate example](/img/attributes/attribute-validate.png)

`[Validate]` runs a C# validation method when a property changes. Invalid input is rejected and the previous value is restored.

```shaderlab
[Validate(KIBA_.KIBAMaterialGUI.Samples.Editor.AllElementsSampleValidator.NonNegativeOnly)]
_Amount ("Amount", Float) = 0
```

## Syntax

```shaderlab
[Validate(Full.Type.Name.MethodName)]
[Validate(Full.Type.Name.MethodName, AssemblyName)]
```

## Validator Shape

Use one of these method shapes:

```csharp
public static bool Method(MaterialGUIPropertyValidateContext context)
public static void Method(MaterialGUIPropertyValidateContext context)
```

Return `false` to reject the edited value. A `void` validator is treated as successful.

## Behavior

- Validators are resolved by fully qualified type and method name.
- Validation runs after a property edit.
- Validators run after an edited value changes, not on ordinary layout or repaint events.
- When validation fails, KIBAMaterialGUI restores each selected material's original property value and keywords, including mixed selections.
- Malformed validator references are reported through diagnostics when possible.

## Example

```csharp
using KIBA_.KIBAMaterialGUI.Editor.UI.Property;

public static class MyShaderValidators
{
    public static bool NonNegative(MaterialGUIPropertyValidateContext context)
        => context.Property.floatValue >= 0f;
}
```

```shaderlab
[Validate(MyShaderValidators.NonNegative)] _Amount ("Amount", Float) = 1
```
