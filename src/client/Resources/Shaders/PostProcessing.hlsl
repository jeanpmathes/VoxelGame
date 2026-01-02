// <copyright file="Post.hlsl" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
    Texture2D    color : register(t0);
    Texture2D    depth : register(t1);
    SamplerState sampler : register(s0);

    /**
     * @brief The parameters for the FXAA post-processing effect.
     */
    struct FXAA
    {
        /**
         * @brief Whether FXAA is enabled.
         */
        bool isEnabled;

        /**
         * @brief The absolute contrast threshold for edge detection.
         */
        float contrastThreshold;

        /**
         * @brief The relative contrast threshold for edge detection.
         */
        float relativeThreshold;

        /**
         * @brief The factor controlling subpixel blending strength.
         */
        float subpixelBlending;

        /**
         * @brief The maximum number of iterations when stepping along an edge.
         */
        int edgeStepCount;

        /**
         * @brief The increment used when stepping along an edge.
         */
        int edgeStep;

        /**
         * @brief A heuristic used to guess the end of an edge.
         */
        float edgeGuess;
    };

    /**
     * @brief The parameters for post-processing effects.
     */
    struct Settings
    {
        /**
         * @brief The FXAA configuration.
         */
        FXAA fxaa;
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

float SampleLuminance(float2 uv)
{
    float4 const color = pp::color.Sample(pp::sampler, uv);
    return native::GetLuminance(saturate(color.rgb));
}

float SampleLuminance(float2 uv, float uOffset, float vOffset, float2 texelSize)
{
    uv += float2(uOffset, vOffset) * texelSize;

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
    float n,       e,      s,  w;
    float ne,      nw,     se, sw;
    float highest, lowest, contrast;
};

LuminanceData SampleLuminanceNeighborhood(float2 uv, float2 texelSize)
{
    LuminanceData l;

    l.m = SampleLuminance(uv);

    l.n = SampleLuminance(uv, NORTH.x, NORTH.y, texelSize);
    l.e = SampleLuminance(uv, EAST.x, EAST.y, texelSize);
    l.s = SampleLuminance(uv, SOUTH.x, SOUTH.y, texelSize);
    l.w = SampleLuminance(uv, WEST.x, WEST.y, texelSize);

    l.ne = SampleLuminance(uv, NORTH.x + EAST.x, NORTH.y + EAST.y, texelSize);
    l.nw = SampleLuminance(uv, NORTH.x + WEST.x, NORTH.y + WEST.y, texelSize);
    l.se = SampleLuminance(uv, SOUTH.x + EAST.x, SOUTH.y + EAST.y, texelSize);
    l.sw = SampleLuminance(uv, SOUTH.x + WEST.x, SOUTH.y + WEST.y, texelSize);

    l.highest  = max(max(max(max(l.n, l.e), l.s), l.w), l.m);
    l.lowest   = min(min(min(min(l.n, l.e), l.s), l.w), l.m);
    l.contrast = l.highest - l.lowest;

    return l;
}

bool ShouldSkip(LuminanceData luminance) { return luminance.contrast < max(pp::settings.fxaa.contrastThreshold, pp::settings.fxaa.relativeThreshold * luminance.highest); }

float DeterminePixelBlendFactor(LuminanceData luminance)
{
    float filter = 2.0f * (luminance.n + luminance.e + luminance.s + luminance.w);
    filter       += luminance.ne + luminance.nw + luminance.se + luminance.sw;
    filter       *= 1.0f / 12.0f;
    filter       = abs(filter - luminance.m);
    filter       = saturate(filter / luminance.contrast);
    filter       = smoothstep(0.0f, 1.0f, filter);
    return filter * filter * pp::settings.fxaa.subpixelBlending;
}

struct Edge
{
    bool  isHorizontal;
    float step;
    float oppositeLuminance;
    float gradient;
};

Edge DetermineEdge(LuminanceData l, float2 texelSize)
{
    Edge e;

    float horizontal = abs(l.n + l.s - 2 * l.m) * 2 + abs(l.ne + l.se - 2 * l.e) + abs(l.nw + l.sw - 2 * l.w);
    float vertical   = abs(l.e + l.w - 2 * l.m) * 2 + abs(l.ne + l.nw - 2 * l.n) + abs(l.se + l.sw - 2 * l.s);

    e.isHorizontal = horizontal >= vertical;
    e.step         = e.isHorizontal ? texelSize.y : texelSize.x;

    float const pLuminance = e.isHorizontal ? l.s : l.e;
    float const nLuminance = e.isHorizontal ? l.n : l.w;
    float const pGradient  = abs(pLuminance - l.m);
    float const nGradient  = abs(nLuminance - l.m);

    if (pGradient < nGradient)
    {
        e.oppositeLuminance = nLuminance;
        e.gradient          = pGradient;
    }
    else
    {
        e.oppositeLuminance = pLuminance;
        e.gradient          = nGradient;
    }

    return e;
}

float DetermineEdgeBlendFactor(LuminanceData luminance, Edge edge, float2 uv, float2 texelSize)
{
    float2 uvEdge = uv;
    float2 edgeStep;

    if (edge.isHorizontal)
    {
        uvEdge.y += edge.step * 0.5;
        edgeStep = float2(texelSize.x, 0.0f);
    }
    else
    {
        uvEdge.x += edge.step * 0.5;
        edgeStep = float2(0.0f, texelSize.y);
    }

    float const edgeLuminance     = (luminance.m + edge.oppositeLuminance) * 0.5f;
    float const gradientThreshold = edge.gradient * 0.25f;

    float2 puv             = uvEdge + edgeStep;
    float  pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
    bool   pAtEnd          = abs(pLuminanceDelta) >= gradientThreshold;

    for (int i = 0; i < pp::settings.fxaa.edgeStepCount && !pAtEnd; i += pp::settings.fxaa.edgeStep)
    {
        puv             += edgeStep;
        pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
        pAtEnd          = abs(pLuminanceDelta) >= gradientThreshold;
    }

    if (!pAtEnd) puv += edgeStep * pp::settings.fxaa.edgeGuess;

    float2 nuv             = uvEdge - edgeStep;
    float  nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
    bool   nAtEnd          = abs(nLuminanceDelta) >= gradientThreshold;

    for (int i = 0; i < pp::settings.fxaa.edgeStepCount && !nAtEnd; i += pp::settings.fxaa.edgeStep)
    {
        nuv             -= edgeStep;
        nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
        nAtEnd          = abs(nLuminanceDelta) >= gradientThreshold;
    }

    if (!nAtEnd) nuv -= edgeStep * pp::settings.fxaa.edgeGuess;

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
    bool  deltaSign;

    if (pDistance <= nDistance)
    {
        shortestDistance = pDistance;
        deltaSign        = pLuminanceDelta >= 0.0f;
    }
    else
    {
        shortestDistance = nDistance;
        deltaSign        = nLuminanceDelta >= 0.0f;
    }

    if (deltaSign == (luminance.m - edgeLuminance >= 0.0f)) return 0.0f;

    return 0.5f - shortestDistance / (pDistance + nDistance);
}

float3 FXAA(float2 uv, float2 texelSize)
{
    LuminanceData luminance = SampleLuminanceNeighborhood(uv, texelSize);

    if (ShouldSkip(luminance)) return pp::color.Sample(pp::sampler, uv).rgb;

    float const pixelBlend = DeterminePixelBlendFactor(luminance);
    Edge const  edge       = DetermineEdge(luminance, texelSize);
    float const edgeBlend  = DetermineEdgeBlendFactor(luminance, edge, uv, texelSize);

    float const finalBlend = max(pixelBlend, edgeBlend);

    if (edge.isHorizontal) uv.y += edge.step * finalBlend;
    else uv.x                   += edge.step * finalBlend;

    return pp::color.Sample(pp::sampler, uv).rgb;
}

float4 PSMain(PSInput const input) : SV_TARGET
{
    float3 color;

    if (!pp::settings.fxaa.isEnabled) color = pp::color.Sample(pp::sampler, input.uv).rgb;
    else
    {
        float width, height;
        pp::color.GetDimensions(width, height);
        float2 const texelSize = float2(1.0f / width, 1.0f / height);

        color = FXAA(input.uv, texelSize);
    }

    return float4(color, 1.0f);
}
