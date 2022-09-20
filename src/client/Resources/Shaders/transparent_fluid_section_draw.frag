#version 430

out vec4 outputColor;

layout(binding = 22) uniform sampler2D accumulationTexture;
layout(binding = 23) uniform sampler2D revealageTexture;

uniform float nearPlane;
uniform float farPlane;

void main()
{
    vec4 accumulated = texelFetch(accumulationTexture, ivec2(gl_FragCoord.xy), 0);
    float revealage = texelFetch(revealageTexture, ivec2(gl_FragCoord.xy), 0).r;

    outputColor = vec4(accumulated.rgb / clamp(accumulated.a, nearPlane, farPlane), revealage);
}
