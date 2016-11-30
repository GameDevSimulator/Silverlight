Shader "Custom/DarknessCompute"
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
				state.b += state.r * 0.05;
				state.b -= _Decay;
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

		// Pass for rendering light affectors
		Pass {
			Tags{ "Queue" = "Opaque" "RenderType" = "Opaque" }
			Blend One One
			//ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
				// Standart empty vertex function
				#pragma vertex vert
				#pragma fragment frag alpha

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;										
					fixed4 color : COLOR;
				};
				
				struct v2f
				{									
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
				};
							
				uniform float _Outline;

				v2f vert(appdata v)
				{					
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

					//float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
					//float2 offset = TransformViewToProjection(norm.xy);

					o.vertex *= 1 + _Outline;					
					o.color = v.color;
					return o;
				}

				float4 frag(v2f i) : COLOR{							
					// Just return Lighten where its rendered
					return float4(0, sqrt(i.color.r) * 1.5, 0, 1);
				}
			ENDCG
		}

		Pass{
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
				// Standart empty vertex function
				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc"

				float4 frag(v2f_img i) : COLOR {
					// just return Darkness;
					return float4(1.0, 0, 0, 1.0);
				}
			ENDCG
		}
	}
}
