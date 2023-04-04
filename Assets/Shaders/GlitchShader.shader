Shader "My Post-Processing/Glitch"
{
	Properties { 
		_MainTex ("Main Texture", 2D) = "white" {}
	}
    SubShader
    {
    	Tags { "RenderType"= "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			sampler2D _MainTex;
            float _GlitchIntensity;
            float4 _GlitchColor;
			struct Attributes
			{
				float4 vertex : POSITION;
            	float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			Varyings vert(Attributes v)
			{
				Varyings o;
				o.vertex = TransformObjectToHClip(v.vertex);
				o.uv = v.uv;
				return o;
			}
            
			float rand(float2 co){
			    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
            float4 frag(Varyings i) : SV_Target {
            	float2 uv = i.uv;//_GlitchIntensity * float2(rand(floor(i.uv*_GlitchIntensity) + _SinTime)-0.5,0.0)/100.0 + i.uv;
				float4 col = tex2D(_MainTex, uv);
            	return col;
            }
            ENDHLSL
        }
    }
    Fallback Off
}