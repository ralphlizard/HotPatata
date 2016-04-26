Shader "Beautiful Dissolves/UI/Dissolve Font" {
	Properties {
		[HideInInspector] _MainTex ("Font Texture", 2D) = "white" {}
		[HideInInspector] _Color ("Text Color", Color) = (1,1,1,1)
		_TilingX ("X", Float) = 10.0
		_TilingY ("Y", Float) = 10.0
		_DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveAmount ("Dissolve Amount", Range(-2.0, 2.0)) = 0.5
		_DirectionMap ("Direction Map", 2D) = "white" {}
		[Toggle(_DISSOLVEGLOW_ON)] _DissolveGlow ("Dissolve Glow", Int) = 1
		_GlowColor ("Glow Color", Color) = (1,0.5,0,1)
		_GlowIntensity ("Glow Intensity", Float) = 10
		_OuterEdgeColor ("Outer Edge Color", Color) = (1,0,0,1)
		_InnerEdgeColor ("Inner Edge Color", Color) = (1,1,0,1)
		_OuterEdgeThickness ("Outer Edge Thickness", Range(0.0, 1.0)) = 0.02
		_InnerEdgeThickness ("Inner Edge Thickness", Range(0.0, 1.0)) = 0.04
		[Toggle(_COLORBLENDING_ON)] _ColorBlending ("Color Blending", Int) = 1
		
		_EdgeColorRamp ("Edge Color Ramp", 2D) = "white" {}
		[Toggle(_EDGECOLORRAMP_USE)] _UseEdgeColorRamp("Use Edge Color Ramp", Int) = 0
	}

	SubShader {

		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
		}
		Lighting Off Cull Off ZTest Always ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _DISSOLVEMAP
			#pragma shader_feature _DIRECTIONMAP
			#pragma shader_feature _DISSOLVEGLOW_ON
			#pragma shader_feature _COLORBLENDING_ON
			#pragma shader_feature _EDGECOLORRAMP_USE
			#include "UnityCG.cginc"
			#include "Assets/BeautifulDissolves/Shaders/Includes/DissolveSpriteFunctions.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color * _Color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				fixed mta = tex2D(_MainTex, IN.texcoord).a;
				fixed4 color = IN.color;

				#ifdef _DISSOLVEMAP
					#if defined(_COLORBLENDING_ON) || defined(_EDGECOLORRAMP_USE)
						fixed totalThickness = _InnerEdgeThickness;
					#else
						fixed totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
					#endif
					half2 adjustedUV = GetAdjustedUV(IN.texcoord);
					fixed dm = tex2D(_DissolveMap, adjustedUV).r;
					SetDissolveLevel(adjustedUV, dm);
					fixed d = dm;
					d -= _DissolveAmount;
					color = AddEdgeColor(color, d, totalThickness);
					color.a *= (d > 0);
					#ifdef _DISSOLVEGLOW_ON
						GetDissolveGlow(dm, d, totalThickness, color);
					#endif
				#endif
				color.a *= mta;
				return color;
			}
			ENDCG
		}
	}
	
	CustomEditor "FontDissolveShaderGUI"
}
