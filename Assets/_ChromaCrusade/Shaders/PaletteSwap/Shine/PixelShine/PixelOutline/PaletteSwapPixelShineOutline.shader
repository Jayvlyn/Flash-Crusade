Shader "Custom/PaletteSwapPixelShineOutline"
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

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (pixels)", Float) = 1
        _UseCorners ("Include Corners (0 = No, 1 = Yes)", Float) = 0

        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _InPalette;
            sampler2D _OutPalette;

            float4 _MainTex_TexelSize;
            float4 _InPalette_TexelSize;

            float4 _Color;

            float _Tolerance;

            float4 _ShineDir;
            float _ShineFrequency;
            float _ShineThreshold;

            float4 _OutlineColor;
            float _OutlineSize;
            float _UseCorners;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.color = v.color * _Color;
                return o;
            }

            float4 SamplePalette(sampler2D tex, float index, float size)
            {
                float u = (index + 0.5) / size;
                return tex2Dlod(tex, float4(u, 0.5, 0, 0));
            }

            float PixelShineMask(float4 screenPos)
            {
                float2 screenUV = screenPos.xy / screenPos.w;

                float2 screenPixel = screenUV * _ScreenParams.xy;

                float2 spritePixelSize = _MainTex_TexelSize.xy * _ScreenParams.xy;

                float2 pixel = floor(screenPixel / spritePixelSize);

                float2 dir = normalize(_ShineDir.xy);

                float v = dot(pixel, dir);

                float stripe = frac(v / _ShineFrequency);

                return step(_ShineThreshold, stripe);
            }

            float OutlineMask(float2 uv)
            {
                float2 px = _MainTex_TexelSize.xy * _OutlineSize;

                float a =
                    tex2D(_MainTex, uv + float2( px.x, 0)).a +
                    tex2D(_MainTex, uv + float2(-px.x, 0)).a +
                    tex2D(_MainTex, uv + float2(0,  px.y)).a +
                    tex2D(_MainTex, uv + float2(0, -px.y)).a;

                if (_UseCorners > 0.5)
                {
                    a +=
                        tex2D(_MainTex, uv + float2( px.x,  px.y)).a +
                        tex2D(_MainTex, uv + float2(-px.x,  px.y)).a +
                        tex2D(_MainTex, uv + float2( px.x, -px.y)).a +
                        tex2D(_MainTex, uv + float2(-px.x, -px.y)).a;
                }

                return a > 0.001;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 baseCol = tex2D(_MainTex, i.uv);

                float centerAlpha = baseCol.a;

                float outline = 0;

                if (centerAlpha <= 0.001)
                    outline = OutlineMask(i.uv);

                if (centerAlpha <= 0.001 && outline > 0.5)
                    return _OutlineColor;

                if (centerAlpha <= 0.001)
                    return float4(0,0,0,0);

                float paletteSize = 1.0 / _InPalette_TexelSize.x;

                float shineMask = PixelShineMask(i.screenPos);

                float4 col = baseCol;

                [loop]
                for (int p = 0; p < 12; p++)
                {
                    if (p >= paletteSize) break;

                    float4 inCol = SamplePalette(_InPalette, p, paletteSize);

                    if (distance(baseCol.rgb, inCol.rgb) <= _Tolerance)
                    {
                        float4 outCol = SamplePalette(_OutPalette, p, paletteSize);
                        outCol.a = baseCol.a;

                        if (p == 0 && shineMask > 0.5)
                            outCol.rgb = float3(1,1,1);

                        col = outCol;
                        break;
                    }
                }

                col *= i.color;

                return col;
            }

            ENDCG
        }
    }
}