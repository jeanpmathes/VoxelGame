#version 430

in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

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

    // Crop plant information.
    bool isUpper = ((aData.y >> 20) & 1) == 1;
    bool isLowered = ((aData.y >> 21) & 1) == 1;
    int type = (aData.y >> 16) & 1;

    // Position
    vec3 position = vec3((aData.x >> 12) & 63, (aData.x >> 6) & 63, aData.x & 63);

    int nShift = (aData.x >> 24) & 3;
    int orientation = (aData.x >> 26) & 1;

    nShift += type * nShift;
    nShift++;

    const float plantShift = 1.0 / 4.0;

    float offset = nShift * (((1 - type) * plantShift) + ((type) * plantShift));

    float xOffset = (1 - orientation) * (offset);
    float zOffset = (orientation) * (offset);

    position.x += xOffset;
    position.z += zOffset;

    if (isLowered) position.y -= 0.0625;

    gl_Position = vec4(position, 1.0) * model * view * projection;
}