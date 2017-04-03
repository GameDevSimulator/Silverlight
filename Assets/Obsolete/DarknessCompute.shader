Shader "Obsolete/DarknessCompute"
{
	Properties{
		_MainTex("Darkness State (RGB)", 2D) = "white" {}
		_MaskTex("Mask (Grayscale)", 2D) = "white" {}		
		_DarkTestRange("Dark Test", Range(0, 1)) = 0.01
		_LightTestRange("Light Test", Range(0, 1)) = 0.01
		_DarkenSpeed("Darken Speed", Range(0, 0.2)) = 0.01
		_LightenSpeed("Lighen Speed", Range(0, 0.6)) = 0.01
		_Outline("Outline", Range(0, 2.0)) = 0.0
		_Decay("Decay", Range(0, 1.0)) = 1.0
	}
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
			// Standart empty vertex function
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _MaskTex;
			uniform float _DarkTestRange;
			uniform float _LightTestRange;
			uniform float _DarkenSpeed;
			uniform float _LightenSpeed;
			uniform float _Decay;

			#define dx(DX) (tex2D(_MainTex, i.uv + float2(DX, 0)).r)

			float4 frag(v2f_img i) : COLOR {
				float4 mask = tex2D(_MaskTex, i.uv);
				float4 state = tex2D(_MainTex, i.uv);
				// state.r = [0,1] value of darkness level (state) 0 - lighten. 1 - darken
				
				float4 state11 = tex2D(_MainTex, i.uv + float2(-_DarkTestRange, -_DarkTestRange));
				float4 state12 = tex2D(_MainTex, i.uv + float2(-_DarkTestRange, _DarkTestRange));
				float4 state21 = tex2D(_MainTex, i.uv + float2(_DarkTestRange, -_DarkTestRange));
				float4 state22 = tex2D(_MainTex, i.uv + float2(_DarkTestRange, _DarkTestRange));
				float4 sum = state11 + state12 + state21 + state22;


				//sum.g = max(0, sum.g - sum.r);

				// State increases by darkness in neighbors and decreases by light
				//float m = min(mask.r, 1.0);
				float delta = sum.r * 0.25 * _DarkenSpeed * mask.r - sum.g * 0.25 * _LightenSpeed;
				state.r += delta;
				state.r = min(state.r, mask.r);
				// R - DARKNESS (0.0 no darkness, 1.0 - darkness)
				// G - LIGHT (0.0 - no light, 1.0 - light)
				// B - NORMAL (0.0 - normal points left, 1.0 - normal points right)

				float normal = (-dx(-0.01) - dx(-0.005) + dx(0.005) + dx(0.01)) * 0.25 + 0.5;
				state.b = normal;				

				//state.b += state.r * 0.05;
				//state.b -= _Decay;
				return state;
			}
			ENDCG
		}

		// Pass for rendering darkness
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
				// Standart empty vertex function
				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform sampler2D _MaskTex;
				uniform float _TestRange;
				uniform float _Speed;
				uniform float _Decay;

				float4 frag(v2f_img i) : COLOR {
					float4 mask = tex2D(_MaskTex, i.uv);
					float4 state = tex2D(_MainTex, i.uv);		

					// just return Darkness;
					//return float4(min(state.r, mask.r), 0, state.b - _Decay, 0);
					state.g = 0;
					return state;
				}
			ENDCG
		}		
	}
}
