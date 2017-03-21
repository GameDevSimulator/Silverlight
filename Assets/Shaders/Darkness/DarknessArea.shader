Shader "Darkness/Area" {
	Properties{
		_MainTexture("Texture", 2D) = "black" {}		
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			Tags{ "Queue" = "Opaque" }
			Cull Back
			Lighting Off
			ZTest LEqual
			ZWrite On
			//ColorMask RGBA

			//Blend SrcAlpha OneMinusSrcAlpha
			Fog{ Mode off }


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"

			
			sampler2D _MainTexture;

			struct appdata
			{
				float4 vertex : POSITION;
				half2 uv : TEXCOORD0;				
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;				
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;				
				return o;
			}

			float4 frag(v2f i) : COLOR
			{				
				fixed4 state = tex2D(_MainTexture, i.uv);
				clip(state.r - 0.01);
				return fixed4(state.r, state.g, state.b, 1);
			}
			ENDCG
		}
	}
}
