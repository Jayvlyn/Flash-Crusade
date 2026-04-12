Shader "TextMeshPro/BitmapShadow"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (pixels)", Float) = 1
        _Corners ("Fill Outline Corners", Float) = 1
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
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"


            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _ClipRect;
            fixed4 _FaceColor;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _Corners;

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
                float4 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.worldPos = v.vertex;
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
                    tex2D(_MainTex, i.uv + float2( 0, -texel.y)).a;

                if (_Corners > 0.5)
                {
                    outline +=
                        tex2D(_MainTex, i.uv + float2( texel.x,  texel.y)).a +
                        tex2D(_MainTex, i.uv + float2(-texel.x,  texel.y)).a +
                        tex2D(_MainTex, i.uv + float2( texel.x, -texel.y)).a +
                        tex2D(_MainTex, i.uv + float2(-texel.x, -texel.y)).a;
                }

                outline = saturate(outline);

                fixed4 face = _FaceColor * i.color;
                fixed4 outlineCol = _OutlineColor * i.color;

                fixed4 col = outlineCol * outline;
                col = lerp(col, face, center);
                col.a *= max(center, outline);

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                return col;
            }
            ENDCG
        }
    }
}