Shader "Obsolete/DarknessInteractor" {
	Properties {
		_MainTex("Mask", 2D) = "white" {}
		_StateColor ("State Color", Color) = (1,0,0,0)		
		_Outline("Outline", Range(0, 2.0)) = 0.0
	}
	SubShader {
		Tags{ "RenderType" = "Opaque" "IgnoreProjector" = "True" }
		LOD 200

		Pass
		{
			Tags { "Queue" = "Opaque" }
			Lighting Off

			Blend One One
			Fog { Mode off }


			CGPROGRAM
			// Standart empty vertex function
			#pragma vertex vert
			#pragma fragment frag alpha

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				half2 uv_MaskTex : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			uniform float _Outline;
			uniform float4 _StateColor;
			uniform sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				//float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				//float2 offset = TransformViewToProjection(norm.xy);

				o.vertex *= 1 + _Outline;
				o.color = v.color;
				o.uv = v.uv_MaskTex;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float mask = tex2D(_MainTex, i.uv).r * i.color.r;
				//float4 c = _StateColor * sqrt(i.color.r) * mask;
				float4 c = _StateColor * sqrt(mask);
				return float4(c.r, c.g, c.b, 1);				
			}
			ENDCG
		}		
	}
}
