Shader "Unlit/Ice"
{
    Properties
    {
        [MainTexture]_MainTex ("Texture", 2D) = "white" {}
        _IceTex ("IceTex", 2D) = "white" {}
        _Alpha("alpha", Range(0, 1)) = 1
        _lineWidth("lineWidth", float) = 0
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
				float2 uv_Ice : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _MainTex_TexelSize;

            sampler2D _IceTex;
            float4 _IceTex_ST;

            float _Alpha;
            float _lineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_Ice = TRANSFORM_TEX(v.uv, _IceTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex_col = tex2D(_MainTex, i.uv);
                fixed4 gray_col = (tex_col.r + tex_col.g + tex_col.b)/3;
                gray_col = gray_col*gray_col;

                fixed4 ice_col = tex2D(_IceTex, i.uv_Ice);
                fixed4 white_col = fixed4(0.62, 0.77, 0.93, 1);
                
                float w = 1;
                if(_lineWidth>0)
                {
                    // 采样周围4个点
                    float2 up_uv = i.uv + float2(0,1) * _lineWidth * _MainTex_TexelSize.xy;
                    float2 down_uv = i.uv + float2(0,-1) * _lineWidth * _MainTex_TexelSize.xy;
                    float2 left_uv = i.uv + float2(-1,0) * _lineWidth * _MainTex_TexelSize.xy;
                    float2 right_uv = i.uv + float2(1,0) * _lineWidth * _MainTex_TexelSize.xy;
                    // 如果有一个点透明度为0 说明是边缘
                    w = tex2D(_MainTex, up_uv).a * tex2D(_MainTex, down_uv).a * tex2D(_MainTex, left_uv).a * tex2D(_MainTex, right_uv).a;
                }
                fixed4 col = lerp(white_col, ice_col, (1-gray_col.r)*w);
                return fixed4(col.rgb, tex_col.a*_Alpha);
                //return tex_col;
            }
            ENDCG
        }
    }
}
