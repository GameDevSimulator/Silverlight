// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Animated"
{
	Properties
	{
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
		_MainTex ("Texture", 2D) = "white" {}
		_RowTiles("Tiles in row", Float) = 10.0
		_ColTiles("Tiles in column", Float) = 10.0
		_Speed("Animation Speed", Range(-50.0,50.0)) = 1.0
		_TransparentCutout("Cutout Color", Range(0.0,50.0)) = 1.0
		_BackgroundColor("BG Color", Color) = (0.0,0.0,0.0,1.0)
		_SrcBlend("Src Blend", Int) = 0
		_DstBlend("Dst Blend", Int) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			//ZWrite On									
			Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcAlpha
			//Blend [_SrcBlend] [_DstBlend]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _BackgroundColor;
			float _TransparentCutout;
			float _RowTiles;
			float _ColTiles;
			float _Speed;
			
			float gauss(float x, float spread)
			{
				return (1.0 / (spread * 2.50662827)) * exp(-x * x / (2.0 * spread * spread));
			}

			v2f vert (appdata v)
			{
				v2f o;
				
				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				float scaleX = length(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 0.0)));
				float scaleY = length(mul(unity_ObjectToWorld, float4(0.0, 1.0, 0.0, 0.0)));
				o.vertex = mul(
					UNITY_MATRIX_P, 
					mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) 
					- float4(v.vertex.x * scaleX, v.vertex.y * scaleY, 0.0, 0.0));


				o.uv = v.uv;

				
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				int r = fmod(_Time.z * _Speed, _RowTiles);
				int c = fmod(_Time.z * _Speed / _RowTiles, _ColTiles);
				// sample the texture
				float2 d = float2(1.0 / _RowTiles, 1.0 / _ColTiles);
				fixed4 texcol = tex2D(_MainTex, i.uv.xy * d + float2(r, -c) * d);

				//fixed4 col =  * _Color;
				//col += (_Time.z * _Speed) % 1.0;

				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//texcol.a = clamp(length(texcol.rgb) * _TransparentCutout, 0.0, 1.0);
				//clip(saturate(texcol - _BackgroundColor));
				return texcol * _Color;
			}
			ENDCG
		}
	}
}
