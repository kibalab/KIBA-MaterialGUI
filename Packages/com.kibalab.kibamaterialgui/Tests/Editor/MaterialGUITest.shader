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
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _ REVIEW_FIRST REVIEW_SECOND
            float4 Vert(float4 vertex : POSITION) : SV_POSITION { return vertex; }
            fixed4 Frag() : SV_Target { return fixed4(1, 1, 1, 1); }
            ENDCG
        }
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"
}


