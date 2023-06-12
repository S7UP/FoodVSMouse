Shader "Unlit/Hide"
{
    Properties
    {
        [MainTexture]_MainTex ("Texture", 2D) = "white" {}
        _FogTex ("FogTex", 2D) = "white" {}
        _Alpha("alpha", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "QUEUE" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // 传统透明度
        ZWrite Off
        LOD 100

        Pass
        {
            

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float2 uv_Fog : TEXCOORD1;
				float2 uv_Noise : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _MainTex_TexelSize;

            sampler2D _FogTex;
            float4 _FogTex_ST;

            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_Fog = TRANSFORM_TEX(v.uv, _FogTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex_col = tex2D(_MainTex, i.uv);
                fixed4 gray_col = (tex_col.r + tex_col.g + tex_col.b)/3;
                //gray_col = gray_col*gray_col;
                gray_col = gray_col + (gray_col*gray_col)/(1-gray_col);
                //fixed4 gray_col = tex_col*0.5;

                fixed4 fog_col = tex2D(_FogTex, i.uv_Fog);

                fixed4 col = lerp(tex_col, fog_col*0.25, 1 - gray_col.r);
                return fixed4(col.rgb, tex_col.a*_Alpha);
                //return fixed4(gray_col.rgb, tex_col.a);
            }
            ENDCG
        }
    }
}
