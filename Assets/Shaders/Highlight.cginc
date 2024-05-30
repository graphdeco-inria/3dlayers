// Highlighting functions

float mod(float x, float y)
{
    return x - y * floor(x / y);
}

float4 ContrastingGreyscaleColor(float4 baseColor)
{
    float greyscale = 0.299 * baseColor.r + 0.587 * baseColor.g + 0.114 * baseColor.b;
    float lineColor = 1.0 - greyscale;
    return float4(lineColor, lineColor, lineColor, 1.0);
}


float4 HighlightGrid(float4 pos, float time, float4 lineColor, float timeFreq)
{
    //float scale = 20.0;
    //float lineWidth = 0.1;
    float scale = 100.0;
    float lineWidth = 0.05;
    //float gridOpacity = sin(_Time.w * 10);
    if (
        //mod(_Time.w * 0.5, 1) < 0.2 &&
            (
            mod(pos.x * scale, 1) < lineWidth ||
            mod(pos.y * scale, 1) < lineWidth ||
            mod(pos.z * scale, 1) < lineWidth
            )
        )
    {
        float gridOpacity = 0.5f * (1.0 + cos(time * timeFreq));
        lineColor.a *= gridOpacity;
        return lineColor;
    }
    else
        return 0.0;
}