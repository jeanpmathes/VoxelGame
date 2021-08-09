#version 430

in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float time;

#pragma include("noise")

void main()
{
    // Normal.
    normal = vec3(0, 0, 0);

    // Texture Index
    texIndex = aData.y & 8191;

    // Texture Coordinate
    int u = (aData.x >> 31) & 1;
    int v = (aData.x >> 30) & 1;
    texCoord = vec2(u, v);

    // Tint
    tint = vec4(((aData.y >> 29) & 7) / 7.0, ((aData.y >> 26) & 7) / 7.0, ((aData.y >> 23) & 7) / 7.0, 1.0);

    // Cross plant information.
    bool isUpper = ((aData.y >> 20) & 1) == 1;
    bool isLowered = ((aData.y >> 21) & 1) == 1;
    bool hasUpper = ((aData.y >> 22) & 1) == 1;

    // Position
    vec3 position = vec3((aData.x >> 10) & 31, (aData.x >> 5) & 31, aData.x & 31);
    int orientation = (aData.x >> 28) & 1;

    float xOffset = (u == 0 ? +1 : -1) * 0.145;
    float zOffset = (u == 0 ? -1 : +1) * 0.145;
    if (orientation == 1) zOffset = xOffset;

    position.x += xOffset;
    position.z += zOffset;

    if (isLowered) position.y -= 0.0625;

    // Sway in wind.
    const float swayAmplitude = 0.1;
    const float swaySpeed = 0.8;

    vec3 wind = vec3(0.7, 0, 0.7);
    float swayStrength = texCoord.y;
    if (hasUpper) swayStrength = (swayStrength + (isUpper ? 1.0 : 0.0)) / 2.0;

    position += wind * noise(vec2(position.xz + wind.xz * time * swaySpeed)) * swayAmplitude * swayStrength;

    gl_Position = vec4(position, 1.0) * model * view * projection;
}