Shader "Custom/PaletteSwap"
{
	    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _InPalette ("Input Palette", 2D) = "white" {}
        _OutPalette ("Output Palette", 2D) = "white" {}
        _Tolerance ("Color Match Tolerance", Float) = 0.001
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 SamplePalette(sampler2D tex, float index, float size)
            {
                float u = (index + 0.5) / size;
                return tex2Dlod(tex, float4(u, 0.5, 0, 0));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                if (col.a <= 0.001)
                    return col;

                float paletteSize = 1.0 / _InPalette_TexelSize.x;

                [loop]
                for (int p = 0; p < 12; p++)
                {
                    if (p >= paletteSize) break;

                    float4 inCol = SamplePalette(_InPalette, p, paletteSize);

                    if (distance(col.rgb, inCol.rgb) <= _Tolerance)
                    {
                        float4 outCol = SamplePalette(_OutPalette, p, paletteSize);
                        outCol.a = col.a;
                        return outCol;
                    }
                }

                return col;
            }

            ENDCG
        }
    }
}