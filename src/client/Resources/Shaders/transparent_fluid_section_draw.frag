/**
 * Based on: https://learnopengl.com/Guest-Articles/2020/OIT/Weighted-Blended
 * by Mahan Heshmati Moghaddam
 */

#version 430

out vec4 outputColor;

layout(binding = 22) uniform sampler2D accumulationTexture;
layout(binding = 23) uniform sampler2D revealageTexture;

uniform float nearPlane;
uniform float farPlane;

const float EPSILON = 0.00001f;

bool isApproximatelyEqual(float a, float b)
{
    return abs(a - b) <= (abs(a) < abs(b) ? abs(b) : abs(a)) * EPSILON;
}

float max3(vec3 v)
{
    return max(max(v.x, v.y), v.z);
}

void main()
{
    ivec2 coords = ivec2(gl_FragCoord.xy);

    float revealage = texelFetch(revealageTexture, coords, 0).r;

    if (isApproximatelyEqual(revealage, 1.0f)) discard;

    vec4 accumulation = texelFetch(accumulationTexture, coords, 0);

    if (isinf(max3(abs(accumulation.rgb)))) accumulation.rgb = vec3(accumulation.a);

    vec3 average_color = accumulation.rgb / max(accumulation.a, EPSILON);
    outputColor = vec4(average_color, 1.0f - revealage);
}
