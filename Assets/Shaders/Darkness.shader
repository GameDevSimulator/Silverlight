Shader "Custom/Darkness" {
	Properties{		
		_MainTex("Texture", 2D) = "white" {}
		_DissolveTex("Dissolve Texture", 2D) = "white" {}
		_DissolveValue("Dissolve Value", Range(0.0,1.0)) = 0.0
		_DissolveEdge("Dissolve Alpha Edge", Range(0.0,1.0)) = 0.1
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		_RimPower("Rim Power", Range(0.5,8.0)) = 3.0
		_PulsePower("Pulse Power", Range(0.0,1.0)) = 0.1
		_SrcBlend("Src Blend", Int) = 0
		_DstBlend("Dst Blend", Int) = 0
		_RandomValue("Random Value", Range(0.0,1.0)) = 0.0
	}
	SubShader{
		Tags{ 
			"Queue" = "AlphaTest"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
			
		//Alphatest Greater 0
		ZWrite On ZTest Less Cull Off
		//ColorMask RGB
		Cull Off
		Blend[_SrcBlend][_DstBlend]

		CGPROGRAM
	#pragma surface surf Lambert vertex:vert addshadow
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_DissolveTex;
			float3 viewDir;
			float4 screenPos;
		};

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _DissolveTex;
		float4 _RimColor;
		float _RimPower;
		float _DissolveValue;
		float _DissolveEdge;
		float _PulsePower;
		float _RandomValue;
		
		float gauss(float x, float spread)
		{
			return (1.0 / (spread * 2.50662827)) * exp( -x * x / (2.0 * spread * spread));
		}

		void vert(inout appdata_full v) {
			v.vertex.xyz += v.normal * sin(_Time.z + v.vertex.y) * _PulsePower + _PulsePower * 0.5;			
		}

		void surf(Input IN, inout SurfaceOutput o) {
			half4 dissolve = tex2D(_DissolveTex, IN.uv_DissolveTex + float2(_RandomValue, _RandomValue) + sin(IN.uv_DissolveTex + _Time.z * _RandomValue) * 0.2);
			float d = dissolve.r + 0.1 - _DissolveValue;	

			clip(dissolve.rgb - _DissolveValue);

					
			//o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;

			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;			
			o.Albedo = tex2D(_MainTex, screenUV * 4.0 * float2(1.0 + _SinTime.y * 0.5 , 1.0 +  _CosTime.y * 0.5)).rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));		

			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow(rim, _RimPower) + max(0, _RimColor.rgb * gauss(d, _DissolveEdge) * _DissolveValue);
		}

		ENDCG
	}
	Fallback "Diffuse"
}