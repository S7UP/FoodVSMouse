Shader "Unlit/Dissolve2"

{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _NoiseTex("Noise", 2D) = "white" {}
        _Threshold("Threshold", Range(0.0, 1.0)) = 0.5
        _EdgeLength("Edge Length", Range(0.0, 0.2)) = 0.1
        _RampTex("Ramp", 2D) = "white" {}
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
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uvMainTex : TEXCOORD0;
				float2 uvNoiseTex : TEXCOORD1;
				float2 uvRampTex : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float _Threshold;
			float _EdgeLength;
			sampler2D _RampTex;
			float4 _RampTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvMainTex = TRANSFORM_TEX(v.uv, _MainTex);
				o.uvNoiseTex = TRANSFORM_TEX(v.uv, _NoiseTex);
				o.uvRampTex = TRANSFORM_TEX(v.uv, _RampTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//�ο�
				fixed cutout = tex2D(_NoiseTex, i.uvNoiseTex).r;
				clip(cutout - _Threshold);

				float degree = saturate((cutout - _Threshold) / _EdgeLength);
				fixed4 edgeColor = tex2D(_RampTex, float2(degree, degree));

				fixed4 col = tex2D(_MainTex, i.uvMainTex);
				col.rgb = 0;

				fixed4 finalColor = lerp(edgeColor, col, degree);
				//fixed4 finalColor = lerp(edgeColor, col, degree);
				//return fixed4(col.rgb, 1);
				return fixed4(finalColor.rgb, col.a);
			}
			ENDCG
		}
        }
}