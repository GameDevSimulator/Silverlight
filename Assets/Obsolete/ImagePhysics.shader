Shader "Obsolete/ImagePhysics"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}	
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Blend One One
			Fog{ Mode off }

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag			
			
			#include "UnityCG.cginc"

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

			sampler2D _MainTex;
			float4 _MainTex_ST;			
			
			#define DELTA (0.5 / 512.0)			
			#define DX(I) (tex2D(_MainTex, i.uv + float2((I) * DELTA, 0)).r)
			#define DY(I) (tex2D(_MainTex, i.uv + float2(0, (I) * DELTA)).r)

			fixed4 frag (v2f_img i) : SV_Target
			{			
				fixed4 col = tex2D(_MainTex, i.uv);				
				col.g = 0;
				col.b = 0;	
				col.a = 0.5;
				return col;
			}
			ENDCG
		}
	}
}
