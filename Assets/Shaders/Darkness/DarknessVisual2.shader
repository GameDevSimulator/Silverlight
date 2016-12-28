Shader "Custom/DarknessVisual2" {
	Properties {
		_StateTex("Darkness state", 2D) = "white" {}
		_Noise("Noise (4 channels)", 2D) = "white" {}
		_Speed("Animation Speed", Vector) = (0,0,0,0)
		_Color("Base Color", Color) = (0.26,0.19,0.16,0.0)
		_RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
	}
	SubShader {
		Lighting Off
		Tags{ "Queue" = "Transparent+1"  "RenderType" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		LOD 200
		
		CGPROGRAM		
		#pragma surface surf Lambert alpha

		sampler2D _StateTex;
		sampler2D _Noise;
		fixed4 _Color;
		fixed4 _RimColor;
		fixed4 _Speed;

		struct Input {
			float2 uv_StateTex;
			float2 uv_Noise;
		};		

		void surf (Input IN, inout SurfaceOutput o) {
			fixed2 baseOffset = fixed2(_Time.x * 0.2, - _Time.x);
			fixed4 state = tex2D(_StateTex, IN.uv_StateTex);
			fixed4 noise1 = tex2D(_Noise, IN.uv_Noise * _Speed.w + baseOffset * _Speed.x);
			fixed4 noise2 = tex2D(_Noise, IN.uv_Noise * _Speed.w + baseOffset * _Speed.y);
			fixed4 noise3 = tex2D(_Noise, IN.uv_Noise * _Speed.w + baseOffset * _Speed.z);
			

			//o.Albedo = noise;
			fixed n = 1 / noise1.r * noise2.g * noise3.b;
			

			o.Alpha = state.r - (1 - state.r) * n + state.b * state.b * 0.2;
			//o.Alpha = n;
			o.Albedo = _Color * n;

			fixed4 color = _Color * n;
			fixed rim = (1 - o.Alpha * o.Alpha);
			o.Emission = _RimColor * rim + (1 - rim) * color;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
