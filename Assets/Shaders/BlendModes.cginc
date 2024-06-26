// Blending functions
float4 BlendMode_Normal(float4 base, float4 blend)
{
	return blend;
}

float4 BlendMode_Multiply(float4 base, float4 blend)
{
    float4 mix = base * blend;
    return mix;
}

float4 BlendMode_Screen(float4 base, float4 blend)
{
    float4 mix = 1.0 - (1.0 - base) * (1.0 - blend);
    return mix;
}

float BlendMode_Overlay(float base, float blend)
{
	return (base <= 0.5) ? 2*base*blend : 1 - 2*(1-base)*(1-blend);
}

float4 BlendMode_Overlay(float4 base, float4 blend)
{
    if (blend.a == 0)
        return 0;
    float3 rgbBlend = float3(blend.r / blend.a, blend.g / blend.a, blend.b / blend.a);
	return blend.a * float4(  BlendMode_Overlay(base.r, rgbBlend.r), 
					BlendMode_Overlay(base.g, rgbBlend.g), 
					BlendMode_Overlay(base.b, rgbBlend.b),
                    1.0);
}

float4 Blend(float4 baseColor, float4 blendColor, int mixMode)
{
    if (mixMode == 0)
        return BlendMode_Normal(baseColor, blendColor);
    if (mixMode == 1)
        return BlendMode_Multiply(baseColor, blendColor);
    if (mixMode == 2)
        return BlendMode_Screen(baseColor, blendColor);
    if (mixMode == 3)
        return BlendMode_Overlay(baseColor, blendColor);

	return blendColor;
}