Shader "Volume/Darkness"
{
	Properties
	{
		_MainTexture("State", 2D) = "black" {}
		_Color("Color", Color) = (1,1,1,1)
		_Color2("Color", Color) = (1,1,1,1)
		_Noise("Noise", 2D) = "white" {}
		_Density("DensityToOpacity", float) = 1.0 
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+100" }
		LOD 100

		Pass
		{
			Lighting Off
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
			fixed4 _Color2;
			float _Density;

	
			#define DELTA 0.001

			float darkness(float3 p)
			{
				float scaleX = length(mul(unity_ObjectToWorld, float4(0.5, 0.0, 0.0, 0.0)));
				float2 offset = p.xy - _Time.xy * 0.1 / scaleX + float2(sin(p.y * 5  + _Time.x) * 0.1, 0);
				float noise = tex2D(_Noise, offset * scaleX).r + 0.01;

				float state = tex2D(_MainTexture, p.xy + 0.5).r;					
				float d = 1 - abs(p.z * 2);
				return 1 - state * state * 10 * d * noise * noise;
			}

			fixed4 colorFromMap(float density) 
			{ 
				float opacity = clamp(1 - exp(density * _Density), 0, 1);

				//float opacity = clamp(1 - density * _Density, 0, 1);
				return (_Color * opacity + _Color2 * (1 - opacity)) * float4(1, 1, 1, opacity);
			}
			 
			#define VOLUME_RAYMARCH_STEPS 32
			//#define VOLUME_NO_DEPTH
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