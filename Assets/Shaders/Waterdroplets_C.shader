Shader "My Post-Processing/Waterdroplets_C"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "black" {}
        _OffsetTex ("Offset Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"= "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            sampler2D _MainTex;
            sampler2D _OffsetTex;


            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float depth : TEXCOORD1;
                float4 scrPos : TEXCOORD3;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.scrPos = ComputeScreenPos(o.vertex); // grab position on screen
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 uv =  i.uv;
                uv.y = 1.0 - uv.y;
                float4 col = tex2D(_MainTex, i.uv + tex2D(_OffsetTex, uv).xy*0.02);
                return col;
            }
            ENDHLSL
        }
    }
    Fallback Off
}