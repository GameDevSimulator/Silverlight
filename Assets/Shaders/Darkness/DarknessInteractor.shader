Shader "Darkness/Interactor" {
	Properties {		
		_Color("Color", Color) = (0,0,0,0)
		_Mask("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags{ "RenderType" = "Opaque" "IgnoreProjector" = "True" }
		LOD 200

		Pass
		{
			Tags{ "Queue" = "Opaque" }
			Cull Back
			Lighting Off
			ZTest LEqual			
			ZWrite On
			ColorMask RGBA			
			

			Blend One One		
			Fog{ Mode off }


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag alpha

			#include "UnityCG.cginc" 

			float4 _Color;
			sampler2D _Mask;
			float4x4 _DarknessAreaTransform;

			struct appdata
			{
				float4 vertex : POSITION;								
				half2 uv_Mask : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				fixed4 color : COLOR;				
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(_DarknessAreaTransform, mul(unity_ObjectToWorld, v.vertex));
				o.uv = v.uv_Mask;
				o.color = v.color;
				return o; 
			}

			float4 frag(v2f i) : COLOR
			{ 
				fixed mask = tex2D(_Mask, i.uv).r;
				return fixed4(
					_Color.r * mask, 
					_Color.g * mask, 
					_Color.b, 
					_Color.a * mask);
			}
			ENDCG
		}
	}
}
