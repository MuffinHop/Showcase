Shader "My Post-Processing/Waterdroplets_A"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "black" {}
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

            sampler2D _MainTex;
            #define smoothv 0.1
            #define ballradius 0.0
            #define metaPow 1.0
            #define densityMin 8.0
            #define densityMax 14.0

            float saturate1(float x)
            {
                return clamp(x, 0.0, 1.0);
            }

            float2 rotuv(float2 uv, float angle, float2 center)
            {
                return mul(float2x2(cos(angle), -sin(angle), sin(angle), cos(angle)), (uv - center)) + center;
            }

            float hash(float n)
            {
                return frac(sin(dot(float2(n, n), float2(12.9898, 78.233))) * 43758.5453);
            }

            float metaBall(float2 uv)
            {
                return length(frac(uv) - float2(0.5, 0.5));
            }

            float rand(float co)
            {
                return frac(sin(dot(float2(co, co), float2(12.9898, 78.233))) * 43758.5453);
            }

            float metaNoiseRaw(float2 uv, float density)
            {
                float v = 0.5, metaball0 = 1.2;


                for (int i = 0; i < 12; i++)
                {
                    float inc = float(rand(float(i))) + 1.0;
                    float r1 = hash(15.3548 * inc);
                    float s1 = _Time.x * 0.01 * r1;
                    float2 f1 = float2(0.1, 0.0) * r1;
                    float2 c1 = float2(hash(11.2 * inc) * 20., hash(33.2 * inc)) * 90.0 * rand(float(i)) - s1;
                    float2 uv1 = -rotuv(uv * (1.0 + r1 * v), r1 * 60.0 + s1, c1);
                    float metaball1 = saturate1(metaBall(uv1) * density);

                    metaball0 *= metaball1;
                }

                return pow(metaball0, metaPow);
            }

            float2 hash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));

                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float perlinnoise(in float2 p)
            {
                const float K1 = 0.366025404;
                const float K2 = 0.211324865;

                float2 i = floor(p + (p.x + p.y) * K1);

                float2 a = p - i + (i.x + i.y) * K2;
                float2 o = (a.x > a.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float2 b = a - o + K2;
                float2 c = a - 1.0 + 2.0 * K2;

                float3 h = max(0.5 - float3(dot(a, a), dot(b, b), dot(c, c)), 0.0);

                float3 n = h * h * h * h * float3(dot(a, hash(i + 0.0)), dot(b, hash(i + o)), dot(c, hash(i + 1.0)));

                return dot(n, float3(70.0, 70.0, 70.0));
            }

            float metaNoise(float2 uv)
            {
                float density = lerp(densityMin,densityMax, perlinnoise(uv + float2(0., _Time.x)) * 0.5 + 0.5);
                return 1.0 - smoothstep(ballradius, ballradius + smoothv, metaNoiseRaw(uv, density));
            }

            float4 calculateNormals(float2 uv, float s)
            {
                float offsetX = s / _ScreenParams.x;
                float offsetY = s / _ScreenParams.y;
                float2 ovX = float2(0.0, offsetX);
                float2 ovY = float2(0.0, offsetY);

                float X = (metaNoise(uv - ovX.yx) - metaNoise(uv + ovX.yx));
                float Y = (metaNoise(uv - ovY.xy) - metaNoise(uv + ovY.xy));
                float Z = sqrt(1.0 - saturate1(dot(float2(X, Y), float2(X, Y))));

                float c = abs(X + Y);
                return normalize(float4(X, Y, Z, c));
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                float2 uv2 = uv;
                float4 n = calculateNormals(uv2, smoothstep(0.0, 0.1, 1.0));
	            n.xyz += tex2D(_MainTex,uv).xyz*0.95;
                n.b=0.0;
                return float4(n.xyz, 1.0);
            }
            ENDHLSL
        }
    }
}