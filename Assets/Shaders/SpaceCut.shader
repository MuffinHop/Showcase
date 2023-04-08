Shader "Custom/SpaceCut"
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
            #define MAX_STEPS 200
            #define MAX_DIST 100
            #define SURF_DIST 1e-4
            
            float sdBox( float3 p, float3 b )
            {
              float3 q = abs(p) - b;
              return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
            }
            float sdSphere( float3 p, float s ){
              return length(p)-s;
            }
            float smin( float a, float b, float k ) {
                float res = exp( -k*a ) + exp( -k*b );
                return -log( res )/k;
            }
            float GetDistance(float3 position)
            {
                float3 _position = position;
                float3 _position2 = position - float3(0.0,3.0,0.0);
                _position.x += _position.y/4.0;
                _position.x += _Time.y;
                _position.x = abs(_position.x%2.0) - 1.0;
                float d = sdBox(_position,float3(0.75,1111.0,1.0));
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
            
            float AO(float3 rayOrigin, float3 rayDirection) {
                float AO=0.0;
                for(float i=1.0; i<20.0; i++) {
                    AO += GetDistance( 
                        rayOrigin + 
                        i * 0.002 * rayDirection);
                }
                return max(AO,0.0);
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
            half4 frag(Varyings IN) : SV_Target
            {
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
                    if(normal.z>0.99) {
                        color = tex2D(_MainTex,IN.uv);
                    }
                    else
                    {
                        float ao = AO(p, normal);
                        color = float4(tex2D(_MainTex,p.xy + p.zz).rgb/( 1.0+ ao),1.0);
                    }
                } else
                {
                    discard;
                }
                return color;
            }
            ENDCG
        }
    }
}