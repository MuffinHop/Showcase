    Shader "Raymarching/RayMarchingCut"
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
           sampler2D _DepthTex;

            sampler2D _MainTex;
            float4 _CameraPosition;
            float4 _CameraRotation;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = UnityObjectToClipPos(IN.vertex.xyz);
                OUT.uv = IN.uv;
                //OUT.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1.0));
                OUT.rayOrigin = _WorldSpaceCameraPos;
                float4 worldPos = IN.vertex;
                OUT.hitPosition = mul(unity_ObjectToWorld, worldPos);
                OUT.scrPos = ComputeScreenPos(OUT.positionHCS); // grab position on screen
                return OUT;
            }
            #define MAX_STEPS 240
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
                _position.z -= 3.0;
                _position.x += _position.y;
                _position.x = abs(_position.x%1.0)-0.5;
                float d = sdBox(_position,float3(0.125,11.0,2.0));
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
                float4 scrPos = IN.scrPos;
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_DepthTex, UNITY_PROJ_COORD(scrPos)));
                float3 rayOrigin = float3(0.0,0.0,-3.0);//_CameraPosition.xyz;
                float3 rayDirection = normalize(float3(IN.uv.x,IN.uv.y,1.0));//normalize(float3((IN.scrPos.xy-float2(0.5,0.5))*float2(1.0,_ScreenParams.y/_ScreenParams.x),1.0));

                float rmSceneDistance = Raymarch(rayOrigin, rayDirection);
                float3 p = rayOrigin + rmSceneDistance * rayDirection;
                half4 color = 0;
                if(rmSceneDistance<MAX_DIST){
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