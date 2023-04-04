Shader "Raymarching/Raymarcher"
{
	Properties { 
		_MainTex ("Main Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque"  }
		LOD 200

		Pass
		{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
    

			#include "UnityCG.cginc"
            struct Attributes
            {
				float4 vertex : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 rayOrigin    : TEXCOORD0;
                float3 hitPosition  : TEXCOORD1;
                float4 scrPos       : TEXCOORD2;
                float2 uv           : TEXCOORD3;
            };
           sampler2D _CameraDepthTexture;

            #define PARTICLE_MAX 50
            float4 DropletPositionArray[PARTICLE_MAX];
            float DropletSizeArray[PARTICLE_MAX];
            sampler2D _MainTex;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = UnityObjectToClipPos(IN.vertex.xyz);
                OUT.uv = IN.uv;
                //OUT.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1.0));
                OUT.rayOrigin = _WorldSpaceCameraPos;
                float4 worldPos = IN.vertex;
                OUT.hitPosition =mul(unity_ObjectToWorld, worldPos);
                OUT.scrPos = ComputeScreenPos(OUT.positionHCS); // grab position on screen
                return OUT;
            }
            #define MAX_STEPS 24
            #define MAX_DIST 32
            #define SURF_DIST 1e-4
            
            float sdSphere( float3 p, float s ){
              return length(p)-s;
            }
            float smin( float a, float b, float k ) {
                float res = exp( -k*a ) + exp( -k*b );
                return -log( res )/k;
            }
            float GetDistance(float3 position)
            {
                float d = 1e6; //length(position) - 0.5;
                for(int i = 0; i < PARTICLE_MAX; i++)
                {
                    d = smin(d, length(position - DropletPositionArray[i].xyz) - DropletSizeArray[i]/2.0, 3.0);
                }
                return d;
             }
            float Raymarch(float3 rayOrigin, float3 rayDirection)
            {
                float rayDistance = 0.0;
                float distanceToSurface;
                for(int i = 0; i < MAX_STEPS; i++)
                {
                    float3 currentPosition = rayOrigin + rayDistance * rayDirection;
                    distanceToSurface = GetDistance( currentPosition);
                    rayDistance += distanceToSurface;
                    if(distanceToSurface < SURF_DIST || rayDistance > MAX_DIST)
                    {
                        break;
                    }
                }

                return rayDistance;
            }
            float3 GetNormal( float3 p )
            {
                const float fDelta = SURF_DIST;

                float3 vOffset1 = float3( fDelta, -fDelta, -fDelta);
                float3 vOffset2 = float3(-fDelta, -fDelta,  fDelta);
                float3 vOffset3 = float3(-fDelta,  fDelta, -fDelta);
                float3 vOffset4 = float3( fDelta,  fDelta,  fDelta);

                float f1 = GetDistance( p + vOffset1 );
                float f2 = GetDistance( p + vOffset2 );
                float f3 = GetDistance( p + vOffset3 );
                float f4 = GetDistance( p + vOffset4 );

                float3 normal = vOffset1 * f1 + vOffset2 * f2 + vOffset3 * f3 + vOffset4 * f4;

                return normalize( normal );
            }
            //#define SLOW true
            half4 frag(Varyings IN) : SV_Target
            {
                #if SLOW
                float2 screenUV = IN.scrPos.xy / IN.scrPos.w;
                float m_depth = LinearEyeDepth(tex2D(_CameraDepthTexture, UNITY_PROJ_COORD(screenUV)));
                //half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float3 rayOrigin = IN.rayOrigin;
                float3 rayDirection = normalize(IN.hitPosition - IN.rayOrigin);

                float rmSceneDistance = Raymarch(rayOrigin, rayDirection);
                float3 p = rayOrigin + rmSceneDistance * rayDirection;
                half4 color = 0;
                if(rmSceneDistance < MAX_DIST)
                {
                    float3 normal = GetNormal( p);
                    normal = mul((float3x3)UNITY_MATRIX_V, normal);
                    //normal = mul(unity_ObjectToWorld, float4(normal,1.0)); //same normal stays despite transformation of the object.
                    color = float4( normal, 1.0);
                } else
                {
                    discard;
                }
                /*
                if (m_depth < rmSceneDistance)
                {
                    discard;
                }*/
                return color;
                #else
                float4 col = tex2D(_MainTex, IN.uv);
                if(col.a < 0.001)
                {
                    discard;
                }
            	return col;
                #endif
            }
            ENDCG
        }
    }
}