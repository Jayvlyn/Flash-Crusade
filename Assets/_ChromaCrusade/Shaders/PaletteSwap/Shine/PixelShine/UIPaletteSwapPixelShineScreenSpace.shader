Shader "Custom/UI/UIPaletteSwapPixelShine"
{
    Properties
    {
        [PerRendererData]_MainTex ("Main Texture", 2D) = "white" {}
        _InPalette ("Input Palette", 2D) = "white" {}
        _OutPalette ("Output Palette", 2D) = "white" {}
        _Tolerance ("Color Match Tolerance", Float) = 0.001
        _Color ("Tint", Color) = (1,1,1,1)

        _ShineDir ("Shine Direction", Vector) = (0.7,0.7,0,0)
        _ShineFrequency ("Shine Frequency", Float) = 40
        _ShineThreshold ("Shine Threshold", Range(0,1)) = 0.85

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local __ UNITY_UI_CLIP_RECT
            #pragma shader_feature_local __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            sampler2D _InPalette;
            sampler2D _OutPalette;

            float4 _MainTex_ST;
            float4 _InPalette_TexelSize;
            float4 _MainTex_TexelSize;

            float4 _ClipRect;

            fixed4 _Color;
            float _Tolerance;

            float4 _ShineDir;
            float _ShineFrequency;
            float _ShineThreshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);

                return o;
            }

            float4 SamplePalette(sampler2D tex, float index, float size)
            {
                float u = (index + 0.5) / size;
                return tex2Dlod(tex, float4(u,0.5,0,0));
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

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                if (col.a <= 0.001)
                    return col;

                float paletteSize = 1.0 / _InPalette_TexelSize.x;

                float shineMask = PixelShineMask(i.screenPos);

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
                            outCol.rgb = float3(1,1,1);

                        col = outCol * i.color;
                        break;
                    }
                }

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}