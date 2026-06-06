# Validate

![Validate の例](/img/attributes/attribute-validate.png)

`[Validate]` は、値が想定範囲やプロジェクトルールから外れているときに警告を表示するための Attribute です。

```shaderlab
[Validate(Min, 0)] _Intensity ("Intensity", Float) = 1
```

値を強制的に変更するものではありません。問題のある値をマテリアル編集者に知らせるために使います。

プロジェクト固有の検証は `IMaterialGUIDiagnosticProvider` で追加できます。
