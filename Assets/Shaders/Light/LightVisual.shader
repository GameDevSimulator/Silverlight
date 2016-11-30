Shader "Custom/LightVisual" {
	Properties{
		_MainTex("Main tex", 2D) = "white"{}
		_Color("Main color", Color) = (1.0, 1.0, 1.0, 1.0)		
		_R("Radius", Range(0, 10)) = 1.0	
	}
	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent"  }
		Blend SrcAlpha OneMinusSrcAlpha					
		ZWrite Off		
		
		//ZTest Less
		Fog{ Mode off }						

		Pass {					
			CGPROGRAM
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

			float4 _Color;
			float _R;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				return o;
			}

			float4 frag(v2f i) : COLOR{				
				float d = 1 - i.color.r;
				float r = _R;
				return float4(_Color.r, _Color.g, _Color.b, _Color.a / (1 + 0.5 * r * d + 0.5 * r * r * d * d));
			}
			ENDCG
		}
	}
	Fallback "Transparent/Diffuse"
}
