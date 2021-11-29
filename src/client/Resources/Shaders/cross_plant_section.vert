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
#pragma include("decode")

void main()
{
    normal = vec3(0, 0, 0);
    texIndex = dc_texIndex();

    // Texture Coordinate.
    int u = dc_i1(aData.x, 31);
    int v = dc_i1(aData.x, 30);
    texCoord = vec2(u, v);

    tint = dc_tint(aData.y, 23);

    // Cross plant information.
    bool isUpper = dc_bool(aData.y, 20);
    bool isLowered = dc_bool(aData.y, 21);
    bool hasUpper = dc_bool(aData.y, 22);

    // Position.
    vec3 position = vec3(dc_i5(aData.x, 10), dc_i5(aData.x, 5), dc_i5(aData.x, 0));
    int orientation = dc_i1(aData.x, 28);

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
