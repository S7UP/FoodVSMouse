Shader "ChangeColor/Water"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _IceTex ("IceTex", 2D) = "white" {} 
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
                float2 uv : TEXCOORD0;
                float2 texcoord:TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                half4 pos:SV_POSITION;
                float2 uv:TEXCOORD0;
                fixed4 color : COLOR;
                float2 uv_Ice : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _IceTex;
            float4 _IceTex_ST;

            fixed _Alpha;
            fixed _CutRateY;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv_Ice = TRANSFORM_TEX(v.uv, _IceTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                clip(_CutRateY - i.uv.y);

                fixed4 tex_col = tex2D(_MainTex, i.uv);
                fixed4 gray_col = (tex_col.r + tex_col.g + tex_col.b)/3;
                gray_col = gray_col*gray_col;

                fixed4 ice_col = tex2D(_IceTex, i.uv_Ice);
                fixed4 white_col = fixed4(0.15, 0.27, 0.59, 1);

                fixed4 col = lerp(white_col, ice_col, 1-gray_col.r);
                //fixed4 col = lerp(white_col, ice_col, 0);
                return fixed4(col.rgb, tex_col.a*_Alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
