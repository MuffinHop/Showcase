Shader "Custom/VertexMovement"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
 
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
 
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
 
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
 
			sampler2D _MainTex;
			float4 _Color;
 
			v2f vert(appdata v)
			{
				v.vertex.xy += normalize(v.vertex.xy) * sin(_Time.y + v.uv.y*12.0) * .3;
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
 
			float4 frag(v2f i) : SV_Target
			{
				float4 c = tex2D(_MainTex, i.uv) * _Color;
				return c;
			}
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
