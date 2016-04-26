#ifndef DISSOLVE_CORE_FUNCTIONS_INCLUDED
#define DISSOLVE_CORE_FUNCTIONS_INCLUDED

half		_GlowIntensity;
half4		_GlowColor;
sampler2D	_DissolveMap;
sampler2D	_DirectionMap;
half		_DissolveAmount;

sampler2D	_SubTex;

half3		_OuterEdgeColor;
half3		_InnerEdgeColor;
half		_OuterEdgeThickness;
half		_InnerEdgeThickness;

sampler2D	_EdgeColorRamp;

//-------------------------------------------------------------------------------------
// Core dissolve functions

void DirectionMapFactor(inout half d, float2 uv)
{
	#ifdef _DIRECTIONMAP
		d += tex2D(_DirectionMap, uv).r;
		d *= 0.5;
	#endif
}

half3 Dissolve(half3 diffColor, float2 uv, half oneMinusReflectivity)
{
#ifdef _DISSOLVEMAP
	half d = tex2D(_DissolveMap, uv).r;
	DirectionMapFactor(d, uv);
	d -= _DissolveAmount;
	
	#if defined(_COLORBLENDING_ON) || defined(_EDGECOLORRAMP_USE)
		half totalThickness = _InnerEdgeThickness;
	#else
		half totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
	#endif
	half3 color = half3(0,0,0);
	
	#ifdef _EDGECOLORRAMP_USE
		color = lerp(diffColor, tex2D(_EdgeColorRamp, float2(clamp(d/(max(0.01, totalThickness)), 0, 1), 0)).rgb, d < totalThickness);
	#else
		#ifdef _COLORBLENDING_ON
			color = lerp(diffColor, lerp(_OuterEdgeColor, _InnerEdgeColor, d/(max(0.01, totalThickness))), d < totalThickness);
		#else
			color = lerp(lerp(diffColor, _InnerEdgeColor, d < totalThickness), _OuterEdgeColor, d < _OuterEdgeThickness);
		#endif
	#endif

	#ifdef _SUBMAP
		color = lerp(color, tex2D(_SubTex, uv).rgb * oneMinusReflectivity, d < 0);
	#else
		clip(d);
	#endif
	return color;
#else
	return diffColor;
#endif
}

half3 DissolveEmission(half3 emis, float2 uv, half3 diffColor)
{
#ifdef _DISSOLVEMAP
	half dm = tex2D(_DissolveMap, uv).r;
	DirectionMapFactor(dm, uv);
	half d = dm;
	half cd = clamp(_DissolveAmount, 0.0, 1.0);
	d -= cd;
	
	#if defined(_COLORBLENDING_ON) || defined(_EDGECOLORRAMP_USE)
		half totalThickness = _InnerEdgeThickness;
	#else
		half totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
	#endif
	#ifdef _DISSOLVEGLOW_ON
		half3 c = _GlowIntensity * _GlowColor.rgb;
		half3 glow = pow(1 - dm, lerp(3, 1, cd)) * cd * c;
		#if defined(_SUBMAP) && defined(_GLOWFOLLOW_ON)
			glow *= lerp(totalThickness < d, (1 - cd), d < 0);
		#else
			#ifndef _SUBMAP
				emis *= (0 < d);
			#endif
			glow *= (totalThickness < d);
		#endif
		emis += glow;
	#endif

	#ifdef _EDGEGLOW_ON
		emis += diffColor * (0 < d) * (d < totalThickness);
	#endif
#endif
	return emis;
}
			
#endif // DISSOLVE_CORE_FUNCTIONS_INCLUDED
