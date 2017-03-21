Shader "Darkness/Combine"
{
	Properties
	{
		_MainTex ("Screen", 2D) = "white" {}
		_StateTex("State", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}

		_Color("Color", Color) = (0.26,0.19,0.16,0.0)
		_EdgeColor("Edge Color", Color) = (0.26,0.19,0.16,0.0)
	}
	SubShader
	{		
		Cull Off 
		ZWrite Off 
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			half4 _MainTex_ST;
	
			sampler2D _StateTex;
			half4 _StateTex_ST;
			

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_StateTex : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv_StateTex : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_StateTex = float2(v.uv_StateTex.x, 1 - v.uv_StateTex.y);				
				return o;
			}
			
			
			fixed4 _Color;
			fixed4 _EdgeColor;
			
			#define RELU(X) (max(0, 2 * (X) - 1))

			#define DELTA 0.002		
			#define DX(I) RELU(tex2D(_StateTex, i.uv_StateTex + float2((I) * DELTA, 0)).r)
			#define DY(I) RELU(tex2D(_StateTex, i.uv_StateTex + float2(0, (I) * DELTA)).r)		

			#define DARKNESS_DISTORTION 0.01
			#define DARKNESS_DARKEN 0.3
			#define DARKNESS_MASK 0.1
			#define NOISE_OFFSET 0.1
			#define LIGHT_LIGHTEN 0.1
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 state = tex2D(_StateTex, i.uv_StateTex);
				float4 noise = (tex2D(_NoiseTex, i.uv + _SinTime.xy * 0.01) - 0.5) * NOISE_OFFSET;
				fixed4 screen = tex2D(_MainTex, i.uv);
				
				fixed4 screenNoisy = tex2D(_MainTex, i.uv + state.r * (noise.xy + float2(_SinTime.w * DARKNESS_DISTORTION, _CosTime.w * DARKNESS_DISTORTION)));
				

				half2 normal = half2(
					0.2 * DX(-2) + 0.8 * DX(-1) - 0.8 * DX(1) - 0.2 * DX(2),
					0.2 * DY(-2) + 0.8 * DY(-1) - 0.8 * DY(1) - 0.2 * DY(2));				

				fixed d = RELU(state.r);
				fixed l = length(normal) * (1 + sin((i.uv.x + _Time.y) * 10) * 0.5);

				screenNoisy *= DARKNESS_DARKEN * state.r;

				fixed4 bias = fixed4(1,1,1,0) * state.g * LIGHT_LIGHTEN 
					+ _EdgeColor * l 
					+ _Color * DARKNESS_MASK * state.a;
				
				
				return screen * (1 - d) + screenNoisy * d + bias;
			}
			ENDCG
		}
	}
}
