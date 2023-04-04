Shader "My Post-Processing/Fluid"
{
	Properties { 
		_MainTex ("Main Texture", 2D) = "black" {}
		_MatCapTex ("MatCap Texture", 2D) = "black" {}
	}
    SubShader
    {
    	Tags { "RenderType"= "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			sampler2D _MainTex;
			sampler2D _MatCapTex;
			sampler2D _FluidRayMarchTex;
			float4 _FluidColor;
			float _FluidIntensity;
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
            
			float2 hash( float2 p ) {
			    p = float2( dot(p,float2(127.1,311.7)),
			              dot(p,float2(269.5,183.3)) );

			    return -1.0 + 2.0*frac(sin(p)*43758.5453123);
			}

			float perlinnoise( in float2 p ) {
			    const float K1 = 0.366025404;
			    const float K2 = 0.211324865;

			    float2 i = floor( p + (p.x+p.y)*K1 );
			    
			    float2 a = p - i + (i.x+i.y)*K2;
			    float2 o = (a.x>a.y) ? float2(1.0,0.0) : float2(0.0,1.0);
			    float2 b = a - o + K2;
			    float2 c = a - 1.0 + 2.0*K2;

			    float3 h = max( 0.5-float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );

			    float3 n = h*h*h*h*float3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));

			    return dot( n, float3(70.0,70.0,70.0) );
			    
			}
			float when_gt(float x, float y) {
			  return max(sign(x - y), 0.0);
			}
			float4 sampleAvg( sampler2D tex, float scale, float2 uv) {
			    
			    float4 sum = tex2D(tex, uv);
				
			    for (float i=0.0; i<3.141592*2.0; i+=0.04) {
			        for (float r = 0.0; r<1.0; r+=0.1) {
			        	float4 sample = tex2D(tex, uv + float2(sin(i),cos(i))*r*scale);
			            sum += sample;
			        }
			    }
			    sum.a = max(sum.a,1.0);
			    return tex2D(tex, uv).a * sum / float4(sum.a,sum.a,sum.a,sum.a);
			}
			//#define DEBUG_FLUID 1
            float4 frag(Varyings i) : SV_Target {
            	#if(DEBUG_FLUID)
					float4 col = tex2D(_MainTex, i.uv);
            		float4 fluid = sampleAvg( _FluidRayMarchTex, _FluidIntensity,i.uv);
            		col = lerp(col,fluid,fluid.a);
            		return col;
            	#else
            		float2 marchUV = tex2D(_FluidRayMarchTex, i.uv).xy;
            		float2 uv = i.uv;
            		float4 blurred = sampleAvg( _FluidRayMarchTex, _FluidIntensity, uv + float2(perlinnoise(i.uv * 12.0  + marchUV + _SinTime.yz),perlinnoise(i.uv * 12.0  + marchUV.y   + _SinTime.yz)) * 0.008	);
            		float alpha = when_gt(length(blurred.rgb),0.02);
            		float frenel = max(1.0 - (blurred.z-0.5)*2.0,0.0) * alpha;
            		uv += (blurred.xy-float2(0.5,0.5)) * 0.583 * alpha + float2(perlinnoise(i.uv*4.0 + marchUV*2.0 + _SinTime.yz),perlinnoise(i.uv*4.0 + marchUV.yx*2.0 + _SinTime.yz)) * alpha *0.03;
            		float4 col = tex2D(_MainTex, uv);
            		col += frenel * _FluidColor;
            		return col;
            	#endif
            }
            ENDHLSL
        }
    }
    Fallback Off
}