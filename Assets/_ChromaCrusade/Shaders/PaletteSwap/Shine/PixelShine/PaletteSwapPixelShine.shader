Shader "Custom/PaletteSwapPixelShine"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _InPalette ("Input Palette", 2D) = "white" {}
        _OutPalette ("Output Palette", 2D) = "white" {}
        _Tolerance ("Color Match Tolerance", Float) = 0.001

        _ShineDir ("Shine Direction", Vector) = (0.7,0.7,0,0)
        _ShineFrequency ("Shine Frequency", Float) = 40
        _ShineThreshold ("Shine Threshold", Range(0,1)) = 0.85
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _InPalette;
            sampler2D _OutPalette;

            float4 _InPalette_TexelSize;
            float _Tolerance;

            float4 _ShineDir;
            float _ShineFrequency;
            float _ShineThreshold;
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float4 SamplePalette(sampler2D tex, float index, float size)
            {
                float u = (index + 0.5) / size;
                return tex2Dlod(tex, float4(u, 0.5, 0, 0));
            }

            float PixelShineMask(float2 uv)
            {
                float2 dir = normalize(_ShineDir.xy);

                float2 pixel = floor(uv / _MainTex_TexelSize.xy);

                float v = dot(pixel, dir);

                float stripe = frac(v / _ShineFrequency);

                return step(_ShineThreshold, stripe);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                if (col.a <= 0.001)
                    return col;

                float paletteSize = 1.0 / _InPalette_TexelSize.x;

                float shineMask = PixelShineMask(i.uv);

                [loop]
                for (int p = 0; p < 12; p++)
                {
                    if (p >= paletteSize) break;

                    float4 inCol = SamplePalette(_InPalette, p, paletteSize);

                    if (distance(col.rgb, inCol.rgb) <= _Tolerance)
                    {
                        float4 outCol = SamplePalette(_OutPalette, p, paletteSize);
                        outCol.a = col.a;

                        if (p == 0 && shineMask > 0.5)
                        {
                            outCol.rgb = float3(1,1,1);
                        }

                        return outCol;
                    }
                }

                return col;
            }

            ENDCG
        }
    }
}