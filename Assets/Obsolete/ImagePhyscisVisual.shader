Shader "Obsolete/PhysicsVisual" 
{
	Properties{
		_MainTex("Main tex", 2D) = "white"{}
		_StateTex("State tex", 2D) = "white"{}
		_Color("Color", Color) = (0.26,0.19,0.16,0.0)
		_EdgeColor("Edge Color", Color) = (0.26,0.19,0.16,0.0)
		_IntersectionColor("Intersection Color", Color) = (0.26,0.19,0.16,0.0)
	}
		SubShader{
		Tags{ "Queue" = "Transparent+1"  "RenderType" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		//Cull Off
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert alpha
		//#pragma target 3.0
		
		sampler2D _MainTex;
		sampler2D _StateTex;
		float4 _Color;
		fixed3 _EdgeColor;
		fixed3 _IntersectionColor;

		struct Input {
			float2 uv_StateTex;
			float2 uv_MainTex;
		};		

		#define DARKNESS_THRESHOLD 0.8
		#define DELTA 0.005		
		#define DX(I) (tex2D(_StateTex, IN.uv_StateTex + float2((I) * DELTA, 0)).r)
		#define DY(I) (tex2D(_StateTex, IN.uv_StateTex + float2(0, (I) * DELTA)).r)		
		#define W(x) (x / 273.0) 
		#define ST(x,y) (tex2D(_StateTex, IN.uv_StateTex + half2((DELTA) * (x),(DELTA) * (y))).r)

		void surf(Input IN, inout SurfaceOutput o) {
			float4 state = tex2D(_StateTex, IN.uv_StateTex);
			half4 bgr = tex2D(_MainTex, IN.uv_MainTex * 2.0);
			
			/*
			float blur = 0;
			blur += ST(-2, -2) * W(1);
			blur += ST(-1, -2) * W(4);
			blur += ST(0, -2) * W(7);
			blur += ST(1, -2) * W(4);
			blur += ST(2, -2) * W(1);

			blur += ST(-2, -1) * W(4);
			blur += ST(-1, -1) * W(16);
			blur += ST(0, -1) * W(26);
			blur += ST(1, -1) * W(16);
			blur += ST(2, -1) * W(4);

			blur += ST(-2, 0) * W(7);
			blur += ST(-1, 0) * W(26);
			blur += ST(0, 0) * W(41);
			blur += ST(1, 0) * W(26);
			blur += ST(2, 0) * W(7);

			blur += ST(-2, 1) * W(4);
			blur += ST(-1, 1) * W(16);
			blur += ST(0, 1) * W(26);
			blur += ST(1, 1) * W(16);
			blur += ST(2, 1) * W(4);

			blur += ST(-2, 2) * W(1);
			blur += ST(-1, 2) * W(4);
			blur += ST(0, 2) * W(7);
			blur += ST(1, 2) * W(4);
			blur += ST(2, 2) * W(1);
			*/


			half2 normal = half2(
				0.2 * DX(-2) + 0.8 * DX(-1) - 0.8 * DX(1) - 0.2 * DX(2), 
				0.2 * DY(-2) + 0.8 * DY(-1) - 0.8 * DY(1) - 0.2 * DY(2));
			fixed l = length(normal) / (1 + bgr.r);		
			fixed r = state.r * state.r;
			fixed rtrue = max(state.r - DARKNESS_THRESHOLD, 0.0) / (1 - DARKNESS_THRESHOLD);
			fixed a = 1.0 - l * (sin(IN.uv_StateTex.x * 10.0 + _Time.w) * cos(IN.uv_StateTex.y * 10.0 + _Time.w));

			fixed isObject = sign(state.a);
			fixed isEdge = sign(max(l - 0.2, 0));			

			//isEdge = isEdge * sin(IN.uv_MainTex.x * 100.0 + _Time.w);

			
			
			
			//o.Alpha = col.r * isObject;
			
			//o.Albedo = float3(isObject, col.g, col.b);
			o.Albedo = l * _EdgeColor * rtrue + r * _Color * bgr.r;
			/*
				isObject * (isEdge * _EdgeColor + (1 - isEdge) * _IntersectionColor) +
				(1 - isObject) * _Color;*/

			o.Alpha = min(1 - a + r * 0.5, 1) + state.a * 0.4;
			//o.Alpha = 1;
			//o.Alpha = abs(length(float2(col.g, col.b) - float2(0.5, 0.5))) + isObject;
		}
		ENDCG
		}
		FallBack "Diffuse"
}
