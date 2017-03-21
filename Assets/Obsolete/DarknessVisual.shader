Shader "Obsolete/DarknessVisual" {
	Properties {
		_MainTex("Main tex", 2D) = "white"{}
		_RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)		
		_StateTex("Darkness state", 2D) = "white" {}						
		_Texture("Darkness Texture", 2D) = "white" {}		
		_AlphaModifier("Alpha", Range(0, 1)) = 0.8
		_WaveSpeed("Wave Speed", Range(0,4)) = 0.5
		_WaveModifier("Wave Modifier", Range(0, 1)) = 0.1
		_WaveSize("Wave Size", Range(0.1, 4)) = 0.5
	}
	SubShader {
		Tags{ "Queue" = "Transparent+1"  "RenderType" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha		
		ZWrite Off
		
		//Cull Off
		LOD 200

		CGPROGRAM		
		#pragma surface surf Lambert alpha
		//#pragma target 3.0
		
		sampler2D _StateTex;
		sampler2D _MainTex;
		sampler2D _Texture;		
		float4 _RimColor;
		float _AlphaModifier;
		float _WaveModifier;
		float _WaveSize;
		float _WaveSpeed;

		struct Input {
			float2 uv_MainTex;			
			float2 uv_StateTex;			
			float2 uv_Texture; 			
			float3 viewDir;
			float4 screenPos;
		};
		
		void surf (Input IN, inout SurfaceOutput o) {	
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			float2 screenUVMoving = screenUV * 4.0 * float2(1.0 + _SinTime.y * 0.5, 1.0 + _CosTime.y * 0.5) * _WaveSpeed;
			
			//fixed4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
			fixed4 state = tex2D(_StateTex, IN.uv_StateTex);
			fixed4 tex = tex2D(_Texture, IN.uv_Texture);				
			//fixed4 tex = tex2D(_Texture, screenUV);
			
			o.Albedo = tex2D(_MainTex, screenUVMoving).rgb;
			o.Albedo *= 0.4;			
			
			o.Alpha = state.r - ((1 - state.r) * tex.r);
				//- (sin(screenUV.x / _WaveSize + _Time.y * _WaveSpeed) * cos(screenUV.y / _WaveSize + _Time.y * _WaveSpeed)) * _WaveModifier * state.r;			
			o.Emission = _RimColor.rgb * (1 - o.Alpha * o.Alpha);
			o.Alpha += state.b * state.b * 0.2;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
