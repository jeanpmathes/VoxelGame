#version 430

in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;
flat out int anim;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // Normal
    int n = (aData.y >> 18) & 7;
    normal = vec3(0.0, 0.0, 0.0);
    normal[((n >> 1) + 3 & 2) | (n >> 2)] = -1.0 + (2 * (n & 1));
    normal.z *= -1.0;
    normal = normalize(normal);

    // Texture Index
    texIndex = aData.y & 8191;

    // Texture Coordinate
    texCoord = vec2((aData.x >> 31) & 1, (aData.x >> 30) & 1);

    // Texture Repetition
    texCoord.x *= ((aData.x >> 24) & 15) + 1;
    texCoord.y *= ((aData.x >> 20) & 15) + 1;

    // Tint
    tint = vec4(((aData.y >> 29) & 7) / 7.0, ((aData.y >> 26) & 7) / 7.0, ((aData.y >> 23) & 7) / 7.0, 1.0);

    // Animation
    anim = (aData.y >> 16) & 1;

    // Position
    vec3 position = vec3((aData.x >> 10) & 31, (aData.x >> 5) & 31, aData.x & 31);
    gl_Position = vec4(position, 1.0) * model * view * projection;
}