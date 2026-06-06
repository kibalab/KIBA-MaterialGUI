Shader "KIBA_/Samples/AllElementsTest"
{
    Properties
    {
        [NoScaleOffset][Group(Textures)] _MainTex("Main Texture", 2D) = "white" {}
        [Group(Textures)] _SampleTexture("Sample Texture (Offset)", 2D) = "white" {}

        [Group(Color)] _SampleColor("Sample Color", Color) = (1,1,1,1)
        [Group(Color)][HDR] _SampleColorHDR("Sample Color (HDR)", Color) = (1,1,1,1)
        [Group(Color)][GradientTexture] _SampleGradient("Sample Gradient", 2D) = "white" {}

        [Group(Numeric)] _SampleFloat("Sample Float", Float) = 1.0
        [Group(Numeric)][Validate(KIBA_.KIBAMaterialGUI.Samples.Editor.AllElementsSampleValidator.NonNegativeOnly)] _ValidatedFloat("Validated Float (>= 0 only)", Float) = 0.5
        [Group(Numeric)][Unit(m)] _SampleFloatUnitM("Sample Float (m)", Float) = 5.0
        [Group(Numeric)][Unit(deg)] _SampleFloatUnitDeg("Sample Float (deg)", Float) = 45.0
        [Group(Numeric)] _SampleRange("Sample Range", Range(0,1)) = 0.5
        [Group(Numeric)][FlexibleRange] _SampleFlexRange("Sample Flex Range", Range(0,1)) = 0.5

        [Group(Vectors)] _SampleVec4("Sample Vec4", Vector) = (1,1,0,0)
        [Group(Vectors)][Vector(2)] _SampleVec2("Sample Vec2", Vector) = (1,0,0,0)
        [Group(Vectors)][Vector(3)] _SampleVec3("Sample Vec3", Vector) = (1,1,1,0)
        [Group(Vectors)][MinMaxSlider(0, 1)] _SampleMinMax01("Sample MinMax (0 to 1)", Vector) = (0.2,0.8,0,0)
        [Group(Vectors)][MinMaxSlider(n1, 1)] _SampleMinMaxSigned("Sample MinMax (-1 to 1)", Vector) = (-0.5,0.5,0,0)

        [Group(Enum)][Enum(UnityEngine.Rendering.CullMode)] _SampleEnum("Sample Enum", Float) = 2
        [Group(Enum)][KeywordEnum(None, Add, Multiply)] _SampleKeywordEnum("Sample Keyword Enum", Float) = 0

        [Group(Enum,Segmented)][Enum(UnityEngine.Rendering.CullMode)][SegmentedEnum] _SampleSegmentedEnum("Sample Segmented Enum", Float) = 2
        [Group(Enum,Segmented)][KeywordEnum(None, Add, Multiply)][SegmentedEnum] _SampleSegmentedKeyword("Sample Segmented Keyword", Float) = 0
        [Group(Enum,Segmented)][Enum(Off, 0, On, 1)][SegmentedEnum] _SampleSegmentedOnOff("Sample Segmented On/Off", Float) = 0

        [Group(Toggle)][Toggle] _SampleToggle("Sample Toggle", Float) = 0
        [Group(Toggle)][ToggleOff(_DETAIL_OFF)] _SampleToggleOff("Sample Toggle (Inverted)", Float) = 1
        [Group(Toggle)][ShowIf(_SampleToggle, 1)] _SampleToggleColor("Visible When Toggle On", Color) = (0.2,0.35,1,1)

        [Group(Decorators)][Space] _SampleSpaceDefault("Sample Space (Default)", Float) = 0
        [Group(Decorators)][Space(32)] _SampleSpaceCustom("Sample Space (32 px)", Float) = 0
        [Group(Decorators)][Divider] _SampleDivider("Sample Divider", Float) = 0
        [Group(Decorators)][DemoNote(Auto_registered_custom_renderer)] _SampleDemoNote("Sample Custom Renderer", Float) = 0.25

        [Group(RenderState)] _Cull("Cull Mode", Float) = 2
        [Group(RenderState)][ShowIf(_Cull, 2)] _CullBackOnlyDetail("Visible When Cull Back", Float) = 1
        [Group(RenderState)] _DoubleSidedEnable("Double Sided", Float) = 0

        _Ungrouped("Ungrouped Float", Float) = 0
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull [_Cull]
        ZWrite On
        ZTest LEqual
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _SampleGradient;

            float4 _SampleColor;
            float _SampleFloat;
            float _SampleRange;
            float4 _SampleVec4;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 guv : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                float2 uv = v.uv * _SampleVec4.xy + _SampleVec4.zw;
                o.uv = uv;
                o.guv = float2(uv.x, 0.5);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                fixed4 gradCol = tex2D(_SampleGradient, i.guv);
                fixed4 tint = _SampleColor;

                float gradLuma = dot(gradCol.rgb, float3(0.299, 0.587, 0.114));
                float mixFactor = saturate(lerp(0.2, 0.8, _SampleRange));

                fixed3 mixed = lerp(baseCol.rgb, gradCol.rgb, mixFactor);
                mixed *= tint.rgb;
                mixed *= max(_SampleFloat, 0.0);
                mixed *= (0.5 + 0.5 * gradLuma);

                return fixed4(mixed, baseCol.a * tint.a);
            }
            ENDCG
        }
    }

    Fallback "Unlit/Color"
}


