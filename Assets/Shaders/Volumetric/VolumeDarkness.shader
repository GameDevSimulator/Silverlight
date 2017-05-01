Shader "Volume/Darkness"
{
	Properties
	{
		_MainTexture("State", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Noise("Noise", 2D) = "white" {}
		_Density("DensityToOpacity", float) = 1.0 
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			//Blend OneMinusDstColor One // Soft Additive
			//Blend DstColor Zero // Multiplicative
			//Blend DstColor SrcColor // 2x Multiplicative
			ZWrite Off
			 
			CGPROGRAM
			#pragma vertex VolumeVert
			#pragma fragment VolumeFrag			


			sampler2D _Noise;
			sampler2D _MainTexture;
			fixed4 _Color;
			float _Density;

	
			#define DELTA 0.01

			float darkness(float3 p)
			{
				float2 offset = p.xy - _Time.xy * 0.1 + float2(sin(p.y * 5 + _Time.x) * 0.1, 0);
				float noise = tex2D(_Noise, offset).r + 0.01;

				float state = 
					tex2D(_MainTexture, p.xy + 0.5 + half2(DELTA, 0)).r +
					tex2D(_MainTexture, p.xy + 0.5 + half2(-DELTA, 0)).r +
					tex2D(_MainTexture, p.xy + 0.5 + half2(0, DELTA)).r +
					tex2D(_MainTexture, p.xy + 0.5 + half2(0, -DELTA)).r;
				state = 0.25 * state;
				float d = 1 - abs(p.z * 2); 				
				return 1 - state * 10 * d * noise * noise;
			}

			fixed4 colorFromMap(float density) 
			{ 
				float opacity = clamp(1 - exp(density * _Density), 0, 1);				
				//float opacity = clamp(1 - density * _Density, 0, 1);
				return _Color * float4(1, 1, 1, opacity);
			}
			 
			#define VOLUME_RAYMARCH_STEPS 16
			//#define VOLUME_NO_JITTERING     
			//#define VOLUME_RAYMARCH_FUNCTION IsosurfaceRaymarch  
			#define VOLUME_MAP darkness
			//#define VOLUME_LIGHT_FUNCTION specular 
			//#define VOLUME_LIGHT_FUNCTION normalDebug
			#define VOLUME_COLOR_FUNCITON colorFromMap  
			#include "Volume.cginc" 

			ENDCG
		}
	}
}