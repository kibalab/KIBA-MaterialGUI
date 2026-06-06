Shader "KIBA_/MaterialGUITests/Conditional"
{
    Properties
    {
        [Group(Root)] _LightingToggle ("Lighting Toggle", Float) = 0
        [Group(Root)][ShowIf(_LightingToggle, 1)] _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        [Group(Root,Child)][Vector(2)] _NestedVector ("Nested Vector", Vector) = (1,1,0,0)
        [Group(Root,Child)] _MainTex ("Main Texture", 2D) = "white" {}
        [ShowIf(_MissingController, 1)] _MissingDependent ("Missing Dependent", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass {}
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
}


