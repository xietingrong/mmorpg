// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:1,cusa:True,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:True,tesm:0,olmd:1,culm:2,bsrc:0,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.4291739,fgcg:0.7618482,fgcb:0.9264706,fgca:1,fgde:0.01,fgrn:16.7,fgrf:53.6,stcl:True,atwp:True,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:1873,x:33229,y:32719,varname:node_1873,prsc:2|emission-1749-OUT,alpha-603-OUT;n:type:ShaderForge.SFN_Multiply,id:1086,x:32812,y:32818,cmnt:RGB,varname:node_1086,prsc:2|A-9592-OUT,B-5983-RGB,C-5376-RGB;n:type:ShaderForge.SFN_Color,id:5983,x:32610,y:32818,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_VertexColor,id:5376,x:32594,y:33050,varname:node_5376,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1749,x:33025,y:32818,cmnt:Premultiply Alpha,varname:node_1749,prsc:2|A-1086-OUT,B-603-OUT;n:type:ShaderForge.SFN_Multiply,id:603,x:33025,y:33029,cmnt:A,varname:node_603,prsc:2|A-9948-OUT,B-5983-A,C-5376-A,D-1637-OUT;n:type:ShaderForge.SFN_ProjectionParameters,id:3834,x:31282,y:33637,varname:node_3834,prsc:2;n:type:ShaderForge.SFN_Subtract,id:6110,x:31493,y:33655,varname:node_6110,prsc:2|A-3834-FAR,B-3834-NEAR;n:type:ShaderForge.SFN_Depth,id:8049,x:31493,y:33526,varname:node_8049,prsc:2;n:type:ShaderForge.SFN_Divide,id:4050,x:31688,y:33596,varname:node_4050,prsc:2|A-8049-OUT,B-6110-OUT;n:type:ShaderForge.SFN_OneMinus,id:3654,x:32125,y:33575,varname:node_3654,prsc:2|IN-4050-OUT;n:type:ShaderForge.SFN_Power,id:1848,x:31853,y:33407,varname:node_1848,prsc:2|VAL-4050-OUT,EXP-8038-OUT;n:type:ShaderForge.SFN_Slider,id:8038,x:31493,y:33374,ptovrint:False,ptlb:Power,ptin:_Power,varname:_Power,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:5;n:type:ShaderForge.SFN_TexCoord,id:4349,x:31312,y:32531,varname:node_4349,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:337,x:32373,y:32259,varname:node_337,prsc:2|A-206-RGB,B-5570-OUT;n:type:ShaderForge.SFN_Vector1,id:5570,x:32131,y:32498,varname:node_5570,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Add,id:9592,x:32614,y:32378,varname:node_9592,prsc:2|A-337-OUT,B-100-OUT,C-4348-OUT;n:type:ShaderForge.SFN_Tex2d,id:206,x:31906,y:32418,varname:_node_206,prsc:2,ntxv:0,isnm:False|UVIN-4349-UVOUT,TEX-1397-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:1397,x:30418,y:32443,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,glob:False,taghide:False,taghdr:False,tagprd:True,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5634,x:31906,y:32633,varname:_node_5634,prsc:2,ntxv:0,isnm:False|UVIN-9510-OUT,TEX-1397-TEX;n:type:ShaderForge.SFN_Vector2,id:600,x:30733,y:32783,varname:node_600,prsc:2,v1:0.01,v2:0.01;n:type:ShaderForge.SFN_Slider,id:5984,x:30377,y:32873,ptovrint:False,ptlb:Bloom,ptin:_Bloom,varname:_Bloom,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:100;n:type:ShaderForge.SFN_Multiply,id:2739,x:31254,y:32880,varname:node_2739,prsc:2|A-4667-OUT,B-5984-OUT,C-7235-OUT;n:type:ShaderForge.SFN_Add,id:9510,x:31623,y:32613,varname:node_9510,prsc:2|A-4349-UVOUT,B-2739-OUT;n:type:ShaderForge.SFN_Multiply,id:100,x:32373,y:32566,varname:node_100,prsc:2|A-5634-RGB,B-7362-OUT;n:type:ShaderForge.SFN_Vector1,id:7362,x:32131,y:32740,varname:node_7362,prsc:2,v1:0.3;n:type:ShaderForge.SFN_Add,id:9948,x:32614,y:32518,varname:node_9948,prsc:2|A-8841-OUT,B-5550-OUT,C-8855-OUT;n:type:ShaderForge.SFN_Multiply,id:8841,x:32373,y:32410,varname:node_8841,prsc:2|A-206-A,B-5570-OUT;n:type:ShaderForge.SFN_Multiply,id:5550,x:32373,y:32706,varname:node_5550,prsc:2|A-5634-A,B-7362-OUT;n:type:ShaderForge.SFN_Add,id:2682,x:31713,y:32885,varname:node_2682,prsc:2|A-4349-UVOUT,B-3716-OUT;n:type:ShaderForge.SFN_Multiply,id:3716,x:31545,y:32977,varname:node_3716,prsc:2|A-2739-OUT,B-7350-OUT;n:type:ShaderForge.SFN_Vector1,id:7350,x:31356,y:33057,varname:node_7350,prsc:2,v1:-1;n:type:ShaderForge.SFN_Tex2d,id:3044,x:31919,y:32885,varname:_node_3044,prsc:2,ntxv:0,isnm:False|UVIN-2682-OUT,TEX-1397-TEX;n:type:ShaderForge.SFN_Vector1,id:9913,x:32131,y:32902,varname:node_9913,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Multiply,id:4348,x:32373,y:32844,varname:node_4348,prsc:2|A-3044-RGB,B-9913-OUT;n:type:ShaderForge.SFN_Multiply,id:8855,x:32373,y:32981,varname:node_8855,prsc:2|A-3044-A,B-9913-OUT;n:type:ShaderForge.SFN_Slider,id:1173,x:30452,y:33063,ptovrint:False,ptlb:BloomLevel,ptin:_BloomLevel,varname:_BloomLevel,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:100;n:type:ShaderForge.SFN_Multiply,id:7235,x:30972,y:32966,varname:node_7235,prsc:2|A-1173-OUT,B-1848-OUT;n:type:ShaderForge.SFN_Slider,id:9590,x:32283,y:33229,ptovrint:False,ptlb:Alpha,ptin:_Alpha,varname:_Alpha,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Add,id:5893,x:32616,y:33435,varname:node_5893,prsc:2|A-9590-OUT,B-3654-OUT;n:type:ShaderForge.SFN_Clamp01,id:1637,x:32812,y:33402,varname:node_1637,prsc:2|IN-5893-OUT;n:type:ShaderForge.SFN_PixelSize,id:7381,x:30566,y:32594,varname:node_7381,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4667,x:31001,y:32778,varname:node_4667,prsc:2|A-600-OUT,B-6826-OUT;n:type:ShaderForge.SFN_Append,id:5702,x:30808,y:32611,varname:node_5702,prsc:2|A-7381-PXH,B-7381-PXW;n:type:ShaderForge.SFN_Vector1,id:6826,x:30733,y:32730,varname:node_6826,prsc:2,v1:0.001;proporder:5983-8038-1397-5984-1173-9590;pass:END;sub:END;*/

Shader "Shader Forge/UITest" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Power ("Power", Range(0, 5)) = 0
        [PerRendererData]_MainTex ("MainTex", 2D) = "white" {}
        _Bloom ("Bloom", Range(0, 100)) = 0
        _BloomLevel ("BloomLevel", Range(0, 100)) = 0
        _Alpha ("Alpha", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _Stencil ("Stencil ID", Float) = 0
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilOpFail ("Stencil Fail Operation", Float) = 0
        _StencilOpZFail ("Stencil Z-Fail Operation", Float) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            Stencil {
                Ref [_Stencil]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Comp [_StencilComp]
                Pass [_StencilOp]
                Fail [_StencilOpFail]
                ZFail [_StencilOpZFail]
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Power;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Bloom;
            uniform float _BloomLevel;
            uniform float _Alpha;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
////// Lighting:
////// Emissive:
                float4 _node_206 = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_5570 = 0.5;
                float node_4050 = (partZ/(_ProjectionParams.b-_ProjectionParams.g));
                float2 node_2739 = ((float2(0.01,0.01)*0.001)*_Bloom*(_BloomLevel*pow(node_4050,_Power)));
                float2 node_9510 = (i.uv0+node_2739);
                float4 _node_5634 = tex2D(_MainTex,TRANSFORM_TEX(node_9510, _MainTex));
                float node_7362 = 0.3;
                float2 node_2682 = (i.uv0+(node_2739*(-1.0)));
                float4 _node_3044 = tex2D(_MainTex,TRANSFORM_TEX(node_2682, _MainTex));
                float node_9913 = 0.2;
                float node_603 = (((_node_206.a*node_5570)+(_node_5634.a*node_7362)+(_node_3044.a*node_9913))*_Color.a*i.vertexColor.a*saturate((_Alpha+(1.0 - node_4050)))); // A
                float3 emissive = ((((_node_206.rgb*node_5570)+(_node_5634.rgb*node_7362)+(_node_3044.rgb*node_9913))*_Color.rgb*i.vertexColor.rgb)*node_603);
                float3 finalColor = emissive;
                return fixed4(finalColor,node_603);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
