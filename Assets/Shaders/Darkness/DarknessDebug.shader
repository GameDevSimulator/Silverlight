Shader "Custom/DarknessDebug" {
	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Opaque" }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag alpha
#include "UnityCG.cginc"

			// vertex input: position, color
			struct appdata {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;		
				return o;
			}

			fixed4 frag(v2f i) : SV_Target { 
				//clip(0.5 + (sin(_Time.z + i.pos.x / 7) * cos(_Time.z + i.pos.y / 5) / 2.0)  - i.color.r);
				//clip(0.7 - i.color.r);
				//i.color.a = i.color.r;
				return float4(i.color.r, 0, i.color.r, 1.0 - i.color.r);
			}
		ENDCG
		}
	}
}
