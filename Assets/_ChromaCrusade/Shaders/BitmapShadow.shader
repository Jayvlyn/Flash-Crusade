Shader "TextMeshPro/BitmapShadow"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (pixels)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Lighting Off
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _FaceColor;
            fixed4 _OutlineColor;
            float _OutlineSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _OutlineSize;

                float center = tex2D(_MainTex, i.uv).a;

                float outline =
                    tex2D(_MainTex, i.uv + float2( texel.x,  0)).a +
                    tex2D(_MainTex, i.uv + float2(-texel.x,  0)).a +
                    tex2D(_MainTex, i.uv + float2( 0,  texel.y)).a +
                    tex2D(_MainTex, i.uv + float2( 0, -texel.y)).a +

                    tex2D(_MainTex, i.uv + float2( texel.x,  texel.y)).a +
                    tex2D(_MainTex, i.uv + float2(-texel.x,  texel.y)).a +
                    tex2D(_MainTex, i.uv + float2( texel.x, -texel.y)).a +
                    tex2D(_MainTex, i.uv + float2(-texel.x, -texel.y)).a;

                outline = saturate(outline);

                fixed4 face = _FaceColor * i.color;
                fixed4 outlineCol = _OutlineColor * i.color;

                fixed4 col = outlineCol * outline;
                col = lerp(col, face, center);
                col.a *= max(center, outline);

                return col;
            }
            ENDCG
        }
    }
}