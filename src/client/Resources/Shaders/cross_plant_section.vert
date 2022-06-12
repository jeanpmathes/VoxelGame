#version 430

in vec3 aVertexPosition;
in vec2 aTexCoord;
in ivec2 aInstanceData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;

uniform mat4 mvp_matrix;

uniform float time;

#pragma include("noise")
#pragma include("decode")

void main()
{
    normal = vec3(0, 0, 0);
    texIndex = dc_texIndex(aInstanceData.y);
    texCoord = aTexCoord;
    tint = dc_tint(aInstanceData.y, 23);

    // Cross plant information.
    bool isUpper = dc_bool(aInstanceData.y, 20);
    bool isLowered = dc_bool(aInstanceData.y, 21);
    bool hasUpper = dc_bool(aInstanceData.y, 22);

    // Position.
    vec3 blockPosition = vec3(dc_i5(aInstanceData.x, 10), dc_i5(aInstanceData.x, 5), dc_i5(aInstanceData.x, 0));
    vec3 vertexPosition = aVertexPosition + blockPosition;
    if (isLowered) vertexPosition.y -= 0.0625;

    // Sway in wind.
    const float swayAmplitude = 0.1;
    const float swaySpeed = 0.8;

    vec3 wind = vec3(0.7, 0.0, 0.7);
    float swayStrength = texCoord.y;
    if (hasUpper) swayStrength = (swayStrength + (isUpper ? 1.0 : 0.0)) / 2.0;

    vertexPosition += wind * noise(vec2(vertexPosition.xz + wind.xz * time * swaySpeed)) * swayAmplitude * swayStrength;

    gl_Position = vec4(vertexPosition, 1.0) * mvp_matrix;
}
