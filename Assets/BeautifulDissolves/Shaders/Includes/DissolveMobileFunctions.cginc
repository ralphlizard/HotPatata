#ifndef DISSOLVE_MOBILE_FUNCTIONS_INCLUDED
#define DISSOLVE_MOBILE_FUNCTIONS_INCLUDED
        
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _DissolveMap;

// Update: Removed direction map to improve performance
// if you still want this feature, it is recommended to bake directions in the dissolve map
//sampler2D _DirectionMap;

sampler2D _SubTex;
fixed _DissolveAmount;
fixed _OuterEdgeThickness;
fixed _InnerEdgeThickness;
fixed3 _OuterEdgeColor;
fixed3 _InnerEdgeColor;
half _GlowIntensity;
fixed3 _GlowColor;

sampler2D _EdgeColorRamp;
		
fixed3 AddEdgeColor(fixed4 mt, fixed d, fixed totalThickness)
{
	#ifdef _EDGECOLORRAMP_USE
		return lerp(mt.rgb, tex2D(_EdgeColorRamp, float2(clamp(d/(max(0.01, totalThickness)), 0, 1), 0)).rgb, d < totalThickness);
	#else
		#ifdef _COLORBLENDING_ON
			return lerp(mt.rgb, lerp(_OuterEdgeColor, _InnerEdgeColor, d/(max(0.01, totalThickness))), d < totalThickness);
		#else
			return lerp(lerp(mt.rgb, _InnerEdgeColor, d < totalThickness), _OuterEdgeColor, d < _OuterEdgeThickness);
		#endif
	#endif
}

void AddSubMap(float2 uv, inout fixed3 color, fixed d)
{
	color = lerp(color, tex2D(_SubTex, uv).rgb, d < 0);
}

half3 GetDissolveGlow(fixed dm, fixed d, fixed totalThickness, fixed3 color)
{
	fixed cd = clamp(_DissolveAmount, 0, 1);
	half3 glow = (1-dm)*(1-dm)*(1-dm) * 2 * cd * _GlowIntensity * _GlowColor.rgb;
	
	#if defined(_SUBMAP) && defined(_GLOWFOLLOW_ON)
		glow *= lerp(totalThickness < d, (1 - cd), d < 0);
	#else
		glow *= totalThickness < d;
	#endif
	
	// Update: Edge glow is now part of dissolve glow for mobile shaders
	glow += color * (0 < d) * (d < totalThickness);
	return glow;
}

#endif
