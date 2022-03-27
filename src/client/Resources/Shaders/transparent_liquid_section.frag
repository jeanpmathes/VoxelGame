#version 430

out vec4 outputColor;

flat in int texIndex;
in vec2 texCoord;

in vec4 tint;
in vec3 normal;
in vec3 worldPosition;

layout(binding = 5) uniform sampler2DArray arrayTexture;
layout(binding = 20) uniform sampler2D depthTex;

uniform float time;
uniform vec3 viewPosition;

float linearize_depth(float z_b, float zNear, float zFar)
{
    float z_n = 2.0 * z_b - 1.0;
    return 2.0 * zNear * zFar / (zFar + zNear - z_n * (zFar - zNear));
}

vec3 saturate(vec3 rgb, float adjustment)
{
    const vec3 W = vec3(0.2125, 0.7154, 0.0721);
    vec3 intensity = vec3(dot(rgb, W));
    return mix(intensity, rgb, adjustment);
}

void main()
{
    vec4 color = texture(arrayTexture, vec3(texCoord, texIndex + int(mod(time * 16, 16))));
    float depth = texelFetch(depthTex, ivec2(int(gl_FragCoord.x), int(gl_FragCoord.y)), 0).x;

    color *= tint;

    float depth_linear = linearize_depth(depth, 0.1, 1000);
    float dist_linear = linearize_depth(gl_FragCoord.z, 0.1, 1000);

    float thickness = abs(depth_linear - dist_linear);

    float fogAmount = clamp(thickness / 4.0, 0.1, 0.9);
    vec4 fogColor = vec4(saturate(color.rgb, 0.8), 1.0);

    float plane = dot(normal, viewPosition - worldPosition);
    bool isAboveWater = plane > 0.0;

    color = isAboveWater ? mix(color, fogColor, fogAmount) : color;

    outputColor = color;
}
