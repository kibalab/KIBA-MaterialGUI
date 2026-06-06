Shader "KIBA_/Samples/InjectionDemo"
{
    Properties
    {
        [Group(General)] _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [Group(Gradient)] _ColorA ("Color A", Color) = (1,0.2,0.2,1)
        [Group(Gradient)] _ColorB ("Color B", Color) = (0.2,0.9,1,1)
        [Group(Gradient)] _GradAngle ("Angle (deg)", Range(0,360)) = 0
        [Group(Wind)] _WindStrength ("Strength", Range(0,2)) = 0.5
        [Group(Wind)] _WindSpeed ("Speed", Range(0,10)) = 1
        [Group(General)] _Cull ("Cull Mode", Float) = 2
    }

    CustomEditor "KIBA_.KIBAMaterialGUI.Editor.MaterialGUI"

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry"
        }
        LOD 100
        Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature _MY_FEATURE

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _ColorA;
            float4 _ColorB;
            float _GradAngle;
            float _WindStrength;
            float _WindSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 wpos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float t = _Time.y * _WindSpeed;
                float wob = sin((v.vertex.x + v.vertex.y) * 0.25 + t) * 0.02 * _WindStrength;
                float2 uv = v.uv + wob;

                o.uv = TRANSFORM_TEX(uv, _MainTex);
                float4 w = mul(unity_ObjectToWorld, v.vertex);
                o.wpos = w.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                float a = radians(_GradAngle);
                float2 dir = float2(cos(a), sin(a));
                float g = dot(i.wpos.xy, dir);
                g = saturate(0.5 + 0.25 * g);

                fixed4 col = lerp(_ColorA, _ColorB, g);

                #ifdef _MY_FEATURE
                col.rgb += 0.15 * saturate(sin(_Time.y * 4) * 0.5 + 0.5);
                #endif

                return tex * col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}


