# クイックスタート

このページでは、パッケージを導入して最初の ShaderGUI を有効にするまでを説明します。

## 1. パッケージを追加する

VPM または Unity Package Manager でプロジェクトに追加します。ランタイム依存はありません。

## 2. CustomEditor を指定する

シェーダーに KIBAMaterialGUI ShaderGUI を指定します。

```shaderlab
CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
```

## 3. Attribute を付ける

```shaderlab
Shader "KIBA_/Examples/QuickStart"
{
    Properties
    {
        [Group(Surface)] _BaseColor ("Base Color", Color) = (1,1,1,1)
        [Group(Surface)] _MainTex ("Main Texture", 2D) = "white" {}

        [Group(Lighting)] [Toggle] _UseLighting ("Use Lighting", Float) = 1
        [Group(Lighting)] [ShowIf(_UseLighting, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
    SubShader { Pass {} }
}
```

## 4. ネストしたグループ

グループ階層はカンマで分けます。

```shaderlab
[Group(Surface, Detail)] _DetailMap ("Detail Map", 2D) = "gray" {}
```

`/` は ShaderLab パーサーと相性が悪いため使用しません。

## 5. 条件付き表示

```shaderlab
[Toggle] _LightingToggle ("Lighting", Float) = 0
[ShowIf(_LightingToggle, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
```

次は [Attribute リファレンス](./attribute-reference.md) を確認してください。

