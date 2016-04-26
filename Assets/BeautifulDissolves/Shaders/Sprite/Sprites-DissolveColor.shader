Shader "Beautiful Dissolves/Sprites/Dissolve Color"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_TilingX ("X", Float) = 1.0
		_TilingY ("Y", Float) = 1.0
		_SubCol ("Substitute Color", Color) = (1,1,1,1)
		_DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveAmount ("Dissolve Amount", Range(-2.0, 2.0)) = 0.5
		_DirectionMap ("Direction Map", 2D) = "white" {}
		[Toggle(_DISSOLVEGLOW_ON)] _DissolveGlow ("Dissolve Glow", Int) = 1
		_GlowColor ("Glow Color", Color) = (1,0.5,0,1)
		_GlowIntensity ("Glow Intensity", Float) = 7
		_OuterEdgeColor ("Outer Edge Color", Color) = (1,0,0,1)
		_InnerEdgeColor ("Inner Edge Color", Color) = (1,1,0,1)
		_OuterEdgeThickness ("Outer Edge Thickness", Range(0.0, 1.0)) = 0.02
		_InnerEdgeThickness ("Inner Edge Thickness", Range(0.0, 1.0)) = 0.04
		[Toggle(_COLORBLENDING_ON)] _ColorBlending ("Color Blending", Int) = 1
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Toggle(_GLOWFOLLOW_ON)] _GlowFollow ("Follow-Through", Int) = 0
		
		_EdgeColorRamp ("Edge Color Ramp", 2D) = "white" {}
		[Toggle(_EDGECOLORRAMP_USE)] _UseEdgeColorRamp("Use Edge Color Ramp", Int) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma shader_feature _DISSOLVEMAP
			#pragma shader_feature _DIRECTIONMAP
			#pragma shader_feature _DISSOLVEGLOW_ON
			#pragma shader_feature _COLORBLENDING_ON
			#pragma shader_feature _EDGECOLORRAMP_USE
			#pragma shader_feature _GLOWFOLLOW_ON
			#define _SUBMAP 1
			#include "UnityCG.cginc"
			#include "Assets/BeautifulDissolves/Shaders/Includes/DissolveSpriteFunctions.cginc"
			
			struct appdata_t
			{
				float4 vertex   	: POSITION;
				float4 color    	: COLOR;
				float2 texcoord 	: TEXCOORD0;
				
			};

			struct v2f
			{
				float4 vertex   	: SV_POSITION;
				fixed4 color    	: COLOR;
				half2 texcoord  	: TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 _SubCol;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 mt = tex2D(_MainTex, IN.texcoord);
				fixed4 color = mt * IN.color;
				
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
					color = lerp(AddEdgeColor(color, d, totalThickness), mt * _SubCol, d < 0);
					
					#ifdef _DISSOLVEGLOW_ON
						GetDissolveGlow(dm, d, totalThickness, color);
					#endif
				#endif
				
				color.a *= (0 < mt.a);
				color.rgb *= color.a;
				
				return color;
			}
		ENDCG
		}
	}
	
	CustomEditor "SpriteDissolveShaderGUI"
}
