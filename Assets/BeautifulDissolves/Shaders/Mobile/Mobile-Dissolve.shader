Shader "Beautiful Dissolves/Mobile/Dissolve" {
	Properties {
		_MainTex ("Albedo", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveAmount ("Dissolve Amount", Range(-2.0, 2.0)) = 0.5
		//_DirectionMap ("Direction Map", 2D) = "white" {}
		_SubTex ("Substitute Texture", 2D) = "white" {}
		[Toggle(_DISSOLVEGLOW_ON)] _DissolveGlow ("Dissolve Glow", Int) = 1
		_GlowColor ("Glow Color", Color) = (1,0.5,0,1)
		_GlowIntensity ("Glow Intensity", Float) = 7
		//[Toggle(_EDGEGLOW_ON)] _EdgeGlow ("Edge Glow", Int) = 1
		[Toggle(_COLORBLENDING_ON)] _ColorBlending ("Color Blending", Int) = 1
		_OuterEdgeColor ("Outer Edge Color", Color) = (1,0,0,1)
		_InnerEdgeColor ("Inner Edge Color", Color) = (1,1,0,1)
		_OuterEdgeThickness ("Outer Edge Thickness", Range(0.0, 1.0)) = 0.02
		_InnerEdgeThickness ("Inner Edge Thickness", Range(0.0, 1.0)) = 0.02
		[Toggle(_GLOWFOLLOW_ON)] _GlowFollow ("Follow-Through", Int) = 0
		
		_EdgeColorRamp ("Edge Color Ramp", 2D) = "white" {}
		[Toggle(_EDGECOLORRAMP_USE)] _UseEdgeColorRamp("Use Edge Color Ramp", Int) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		// Update: ZWrite is disabled to prevent alphas from occluding geometry
		// Back-face culling is also enabled to prevent back-faces from drawing over front-faces
		// and to imporve performance on mobile devices
		ZWrite Off
		//Cull Off
		Blend SrcAlpha OneMinusSrcAlpha	
		LOD 150

		CGPROGRAM
		// Update: Using custom lighter lambert lighting function to improve performance
		// To use old lighting model, change MobileLambert to Lambert on the line below
		#pragma surface surf MobileLambert keepalpha nolightmap noforwardadd
		// Update: Edge glow is now a part of dissolve glow
		//#pragma shader_feature _EDGEGLOW_ON
		#pragma shader_feature _COLORBLENDING_ON
		#pragma shader_feature _EDGECOLORRAMP_USE
		#pragma shader_feature _DISSOLVEGLOW_ON
		#pragma shader_feature _NORMALMAP
		// Update: Direction maps are disabled for mobile shaders to improve performance
		// Directions should now be baked into the dissolve maps red channel directly
		//#pragma shader_feature _DIRECTIONMAP
		#pragma shader_feature _SUBMAP
		#pragma shader_feature _GLOWFOLLOW_ON
		
		#pragma multi_compile __ _DISSOLVEMAP
		
		#include "Assets/BeautifulDissolves/Shaders/Includes/DissolveMobileFunctions.cginc"
		
  		inline fixed4 LightingMobileLambert (SurfaceOutput s, fixed3 lightDir, fixed atten) {
              fixed NdotL = dot (s.Normal, lightDir) * 0.5 + 0.5;
              fixed4 c;
              c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
              c.a = s.Alpha;
              return c;
        }

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 mt = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 color = mt;
			
			#ifdef _DISSOLVEMAP
				#if defined(_COLORBLENDING_ON) || defined(_EDGECOLORRAMP_USE)
					fixed totalThickness = _InnerEdgeThickness;
				#else
					fixed totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
				#endif
				fixed dm = tex2D(_DissolveMap, IN.uv_MainTex).r;
				fixed d = dm;
				d -= _DissolveAmount;
				color.rgb = AddEdgeColor(mt, d, totalThickness);
				
				#ifdef _SUBMAP
					AddSubMap(IN.uv_MainTex, color.rgb, d);
				#else
					color *= (0 < d);
				#endif
				
				#ifdef _DISSOLVEGLOW_ON
					o.Emission = GetDissolveGlow(dm, d, totalThickness, color.rgb);
				#endif
			#endif
			
			o.Albedo = color;
			o.Alpha = color.a;
			
			#ifdef _NORMALMAP
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			#endif
		}
		ENDCG
	}

	Fallback "Mobile/Transparent/VertexLit"
	CustomEditor "MobileDissolveShaderGUI"
}
