Shader "Custom/DarknessDebug" {
	SubShader{
		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
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

	fixed4 frag(v2f i) : SV_Target{ 
		//clip(0.5 + (sin(_Time.z + i.pos.x / 7) * cos(_Time.z + i.pos.y / 5) / 2.0)  - i.color.r);
		clip(0.7 - i.color.r);
		return i.color; 		
	}
		ENDCG
	}
	}
}
