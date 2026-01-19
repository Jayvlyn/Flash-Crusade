Shader "Sprites/PixelOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (pixels)", Float) = 1
        _UseCorners ("Include Corners (0 = No, 1 = Yes)", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;

            float4 _OutlineColor;
            float _OutlineSize;
            float _UseCorners;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 center = tex2D(_MainTex, i.uv);

                // Keep original sprite pixels
                if (center.a > 0.0)
                    return center * i.color;

                float2 px = _MainTex_TexelSize.xy * _OutlineSize;

                // 4-direction sampling
                float a =
                    tex2D(_MainTex, i.uv + float2( px.x, 0)).a +
                    tex2D(_MainTex, i.uv + float2(-px.x, 0)).a +
                    tex2D(_MainTex, i.uv + float2(0,  px.y)).a +
                    tex2D(_MainTex, i.uv + float2(0, -px.y)).a;

                // Optional diagonal (corner) sampling
                if (_UseCorners > 0.5)
                {
                    a +=
                        tex2D(_MainTex, i.uv + float2( px.x,  px.y)).a +
                        tex2D(_MainTex, i.uv + float2(-px.x,  px.y)).a +
                        tex2D(_MainTex, i.uv + float2( px.x, -px.y)).a +
                        tex2D(_MainTex, i.uv + float2(-px.x, -px.y)).a;
                }

                if (a > 0.0)
                    return _OutlineColor;

                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}