// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//--------------------------------------------------------------
// Desc: Lightmap shader(不受实时光照影响，只读取光照烘焙图)
// Author: yaoxin
// Date: 2015-04-16
// Copyright: xingchen
//---------------------------------------------------------------
Shader "StarStudio/Lightmap/Lightmap" 	// 考虑和unity本身的Mobile-Lightmap-Unlit的效率高低比较
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader 
	{
		Tags { "RenderType" = "Opaque" }
		LOD 50
		
		pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			struct vert_data 
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD;
				float4 texcoord1 : TEXCOORD1;
			};
			
			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD;
				float2 lightMapUV : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			v2f vert(vert_data v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.lightMapUV = ((v.texcoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
				UNITY_TRANSFER_FOG(o, o.position);
				o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
			    half3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightMapUV));
				half4 texColor = tex2D(_MainTex,i.uv);
				texColor.rgb *= lightmap;
				
				UNITY_APPLY_FOG(i.fogCoord, texColor);
				
				return texColor * _Color;
			}
			
			ENDCG
		}
	}
}
