//  <copyright file="Post.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

/**
 * @file PostProcessing.hlsl
 * @brief Post-processing shader applying antialiasing effects.
 *
 * The used antialiasing technique is FXAA (Fast Approximate Anti-Aliasing).
 * This implementation is based on the tutorial by Jasper Flick (Catlike Coding).
 */
namespace pp
{
    Texture2D    color   : register(t0);
    Texture2D    depth   : register(t1);
    SamplerState sampler : register(s0);

    /**
     * @brief The settings for the antialiasing post-processing effect.
     */
    struct Settings
    {
        /**
         * @brief The level of antialiasing to apply, from 0 (off) to 3 (ultra).
         */
        int levelOfAntiAliasing;
    };

    ConstantBuffer<Settings> settings : register(b0);
}

struct PSInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
};

PSInput VSMain(float4 const position : POSITION, float2 const uv : TEXCOORD)
{
    PSInput result;

    result.position = position;
    result.uv       = uv;

    return result;
}

struct Settings
{
    float contrastThreshold;
    float relativeThreshold;
    float subpixelBlending;
    int edgeStepCount;
    int edgeStep;
    float edgeGuess;
    float2 texelSize;
};

float SampleLuminance(float2 uv)
{
    float4 const color = pp::color.Sample(pp::sampler, uv);
    return native::GetLuminance(saturate(color.rgb));
}

float SampleLuminance(float2 uv, float uOffset, float vOffset, Settings settings)
{
    uv += float2(uOffset, vOffset) * settings.texelSize;
    
    return SampleLuminance(uv);
}

// DirectX texture coordinates have the origin in the top-left corner.
static float2 const NORTH = float2(0.0f, -1.0f);
static float2 const EAST  = float2(1.0f, 0.0f);
static float2 const SOUTH = float2(0.0f, 1.0f);
static float2 const WEST  = float2(-1.0f, 0.0f);

struct LuminanceData
{
    float m;
    float n, e, s, w;
    float ne, nw, se, sw;
    float highest, lowest, contrast;
};

LuminanceData SampleLuminanceNeighborhood(float2 uv, Settings settings)
{
    LuminanceData l;
    
    l.m = SampleLuminance(uv);
    
    l.n = SampleLuminance(uv, NORTH.x, NORTH.y, settings);
    l.e = SampleLuminance(uv, EAST.x, EAST.y, settings);
    l.s = SampleLuminance(uv, SOUTH.x, SOUTH.y, settings);
    l.w = SampleLuminance(uv, WEST.x, WEST.y, settings);

    l.ne = SampleLuminance(uv, NORTH.x + EAST.x, NORTH.y + EAST.y, settings);
    l.nw = SampleLuminance(uv, NORTH.x + WEST.x, NORTH.y + WEST.y, settings);
    l.se = SampleLuminance(uv, SOUTH.x + EAST.x, SOUTH.y + EAST.y, settings);
    l.sw = SampleLuminance(uv, SOUTH.x + WEST.x, SOUTH.y + WEST.y, settings);

    l.highest = max(max(max(max(l.n, l.e), l.s), l.w), l.m);
    l.lowest = min(min(min(min(l.n, l.e), l.s), l.w), l.m);
    l.contrast = l.highest - l.lowest;
    
    return l;
}

bool ShouldSkip(LuminanceData luminance, Settings settings)
{
    return luminance.contrast < max(settings.contrastThreshold, settings.relativeThreshold * luminance.highest);
}

float DeterminePixelBlendFactor(LuminanceData luminance, Settings settings)
{
    float filter = 2.0f * (luminance.n + luminance.e + luminance.s + luminance.w);
    filter += luminance.ne + luminance.nw + luminance.se + luminance.sw;
    filter *= 1.0f / 12.0f;
    filter = abs(filter - luminance.m);
    filter = saturate(filter / luminance.contrast);
    filter = smoothstep(0.0f, 1.0f, filter);
    return filter * filter * settings.subpixelBlending;
}

struct Edge
{
    bool isHorizontal;
    float step;
    float oppositeLuminance;
    float gradient;
};

Edge DetermineEdge(LuminanceData l, Settings settings)
{
    Edge e;
    
    float horizontal =
        abs(l.n + l.s - 2 * l.m) * 2 +
        abs(l.ne + l.se - 2 * l.e) +
        abs(l.nw + l.sw - 2 * l.w);
    float vertical =
        abs(l.e + l.w - 2 * l.m) * 2 +
        abs(l.ne + l.nw - 2 * l.n) +
        abs(l.se + l.sw - 2 * l.s);
    
    e.isHorizontal = horizontal >= vertical;
    e.step = e.isHorizontal ? settings.texelSize.y : settings.texelSize.x;
    
    float const pLuminance = e.isHorizontal ? l.s : l.e;
    float const nLuminance = e.isHorizontal ? l.n : l.w;
    float const pGradient = abs(pLuminance - l.m);
    float const nGradient = abs(nLuminance - l.m);
    
    if (pGradient < nGradient)
    {
        e.oppositeLuminance = nLuminance;
        e.gradient = pGradient;
    }
    else 
    {
        e.oppositeLuminance = pLuminance;
        e.gradient = nGradient;
    }
    
    return e;
}

float DetermineEdgeBlendFactor(LuminanceData luminance, Edge edge, float2 uv, Settings settings)
{
    float2 uvEdge = uv;
    float2 edgeStep;
    
    if (edge.isHorizontal)
    {
        uvEdge.y += edge.step * 0.5;
        edgeStep = float2(settings.texelSize.x, 0.0f);
    }
    else
    {
        uvEdge.x += edge.step * 0.5;
        edgeStep = float2(0.0f, settings.texelSize.y);
    }

    float const edgeLuminance = (luminance.m + edge.oppositeLuminance) * 0.5f;
    float const gradientThreshold = edge.gradient * 0.25f;

    float2 puv = uvEdge + edgeStep;
    float pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
    bool pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;

    for (int i = 0; i < settings.edgeStepCount && !pAtEnd; i += settings.edgeStep)
    {
        puv += edgeStep;
        pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
        pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
    }

    if (!pAtEnd)
        puv += edgeStep * settings.edgeGuess;

    float2 nuv = uvEdge - edgeStep;
    float nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
    bool nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;

    for (int i = 0; i < settings.edgeStepCount && !nAtEnd; i += settings.edgeStep)
    {
        nuv -= edgeStep;
        nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
        nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
    }

    if (!nAtEnd)
        nuv -= edgeStep * settings.edgeGuess;

    float pDistance, nDistance;
    if (edge.isHorizontal)
    {
        pDistance = puv.x - uv.x;
        nDistance = uv.x - nuv.x;
    }
    else
    {
        pDistance = puv.y - uv.y;
        nDistance = uv.y - nuv.y;
    }

    float shortestDistance;
    bool deltaSign;

    if (pDistance <= nDistance)
    {
        shortestDistance = pDistance;
        deltaSign = pLuminanceDelta >= 0.0f;
    }
    else
    {
        shortestDistance = nDistance;
        deltaSign = nLuminanceDelta >= 0.0f;
    }

    if (deltaSign == (luminance.m - edgeLuminance >= 0.0f))
        return 0.0f;
    
    return 0.5f - shortestDistance / (pDistance + nDistance);
}

float3 FXAA(float2 uv, Settings settings)
{
    LuminanceData luminance = SampleLuminanceNeighborhood(uv, settings);
    
    if (ShouldSkip(luminance, settings))
    {
        return pp::color.Sample(pp::sampler, uv).rgb;
    }

    float const pixelBlend = DeterminePixelBlendFactor(luminance, settings);
    Edge const edge = DetermineEdge(luminance, settings);
    float const edgeBlend = DetermineEdgeBlendFactor(luminance, edge, uv, settings);

    float const finalBlend = max(pixelBlend, edgeBlend);
    
    if (edge.isHorizontal)
    {
        uv.y += edge.step * finalBlend;
    }
    else
    {
        uv.x += edge.step * finalBlend;
    }
    
    return pp::color.Sample(pp::sampler, uv).rgb;
}

float4 PSMain(PSInput const input) : SV_TARGET
{
    float3 color;
    
    if (pp::settings.levelOfAntiAliasing == 0)
    {
        color = pp::color.Sample(pp::sampler, input.uv).rgb;
    }
    else
    {
        Settings settings;

        switch (pp::settings.levelOfAntiAliasing)
        {
        case 1: // Medium Quality
            settings.contrastThreshold = 0.0833f;
            settings.relativeThreshold = 0.333f;
            settings.subpixelBlending = 0.50f;

            settings.edgeStepCount = 4;
            settings.edgeStep = 2;
            settings.edgeGuess = 12.0f;
            break;

        case 2: // High Quality
            settings.contrastThreshold = 0.0625f;
            settings.relativeThreshold = 0.166f;
            settings.subpixelBlending = 0.75f;

            settings.edgeStepCount = 8;
            settings.edgeStep = 2;
            settings.edgeGuess = 8.0f;
            break;

        default: // Ultra Quality
            settings.contrastThreshold = 0.0312f;
            settings.relativeThreshold = 0.063f;
            settings.subpixelBlending = 1.00f;

            settings.edgeStepCount = 12;
            settings.edgeStep = 1;
            settings.edgeGuess = 8.0f;
            break;
        }

        float width, height;
        pp::color.GetDimensions(width, height);
        settings.texelSize = float2(1.0f / width, 1.0f / height);

        color = FXAA(input.uv, settings);
    }

    return float4(color, 1.0f);
}
