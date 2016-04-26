#ifndef DISSOLVE_SPRITE_FUNCTIONS_INCLUDED
#define DISSOLVE_SPRITE_FUNCTIONS_INCLUDED

sampler2D _MainTex;
sampler2D _SubTex;
sampler2D _DissolveMap;
sampler2D _DirectionMap;
fixed _DissolveAmount;
fixed _OuterEdgeThickness;
fixed _InnerEdgeThickness;
fixed4 _OuterEdgeColor;
fixed4 _InnerEdgeColor;
half _GlowIntensity;
fixed4 _GlowColor;
half _TilingX;
half _TilingY;

sampler2D _EdgeColorRamp;


half2 GetAdjustedUV(half2 uv)
{
	return half2(uv.x * _TilingX, uv.y * _TilingY);
}

void SetDissolveLevel(half2 uv, inout fixed d)
{
	#ifdef _DIRECTIONMAP
		d += tex2D(_DirectionMap, uv).r;
		d *= 0.5;
	#endif
}
		
fixed4 AddEdgeColor(fixed4 mt, fixed d, fixed totalThickness)
{
	#ifdef _EDGECOLORRAMP_USE
			return lerp(mt, tex2D(_EdgeColorRamp, float2(clamp(d/(max(0.01, totalThickness)), 0, 1), 0)), d < totalThickness);
	#else
		#ifdef _COLORBLENDING_ON
			return lerp(mt, lerp(_OuterEdgeColor, _InnerEdgeColor, d/(max(0.01, totalThickness))), d < totalThickness);
		#else
			return lerp(lerp(mt, _InnerEdgeColor, d < totalThickness), _OuterEdgeColor, d < _OuterEdgeThickness);
		#endif
	#endif
}

void AddSubMap(fixed4 mt, float2 uv, inout fixed4 color, fixed d)
{
	color = lerp(color, tex2D(_SubTex, uv), d < 0);
}

void GetDissolveGlow(fixed dm, fixed d, fixed totalThickness, inout fixed4 color)
{
	fixed cd = clamp(_DissolveAmount, 0, 1);
	fixed3 glow = pow(1 - dm, lerp(3, 1, cd)) * cd * _GlowIntensity * _GlowColor;

	#if defined(_SUBMAP) && defined(_GLOWFOLLOW_ON)
		glow *= lerp(totalThickness < d, (1 - cd), d < 0);
	#else
		glow *= (totalThickness < d);
	#endif
	color.rgb += glow;
}

#endif
