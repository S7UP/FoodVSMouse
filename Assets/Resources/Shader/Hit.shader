Shader "ChangeColor/Hit"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("add color", Color) = (1,1,1,1)
        _FlashRate("_FlashRate",Range(0 , 0.75)) = 0
        _Alpha("alpha", Range(0 , 1.0)) = 1
        _CutRateY("cutRateY", Range(0, 1.0)) = 0
    }
    SubShader
    {
        Tags { "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex:POSITION;
                float2 texcoord:TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                half4 pos:SV_POSITION;
                float2 uv:TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _FlashRate;//对外参数表示是否被攻击了
            fixed _Alpha;
            fixed _CutRateY;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                clip(i.uv.y - _CutRateY);
                fixed4 tex = tex2D(_MainTex, i.uv);

                if (_FlashRate > 0) {//是否被攻击
                    if (tex.rgba.a == 1)
                    {
                        // 颜色减淡（闪白）
                        tex.rgb = tex.rgb + (tex.rgb * _Color.rgb * _FlashRate) / (1 - _Color.rgb * _FlashRate);
                    }
                }
                tex.a = tex.a*_Alpha;

                return tex;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
