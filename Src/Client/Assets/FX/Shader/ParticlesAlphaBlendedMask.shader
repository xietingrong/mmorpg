// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "xj/Particles/AlphaBlendedMask" 
{
	Properties {
	_TintColor ("TintColor", Color) = (1,1,1,1)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_MaskTex ("Mask", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Off Lighting Off ZWrite Off Fog { Mode Off }

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _MaskTex;
			fixed4 _TintColor;
			
			struct appdata_t {
				half4 vertex : POSITION;
				fixed4 color : COLOR;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f {
				half4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord : TEXCOORD0;
				half2 texcoord_mask : TEXCOORD1;
			};
			
			half4 _MainTex_ST;
			half4 _MaskTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord_mask = TRANSFORM_TEX(v.texcoord,_MaskTex);
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{	
				half4 result = tex2D(_MainTex,i.texcoord) * tex2D(_MaskTex,i.texcoord_mask);
				return 2.0f * i.color * _TintColor * result;
			}
			ENDCG 
		}
	}	
}
}