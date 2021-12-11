#version 430

in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;
flat out ivec2 texCordMax;

out vec4 tint;
flat out int anim;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

#pragma include("decode")

void main()
{
    int n = dc_i3(aData.y, 18);
    normal = dc_sideToNormal(n);

    texIndex = dc_texIndex(aData.y);
    texCoord = dc_texCoord(aData.x, 30);

    // Texture Repetition
    int xLen = dc_i4(aData.x, 24) + 1;
    int yLen = dc_i4(aData.x, 20) + 1;

    texCordMax = ivec2(xLen, yLen);

    texCoord.x *= xLen;
    texCoord.y *= yLen;

    tint = dc_tint(aData.y, 23);

    anim = dc_i1(aData.y, 16);

    // Position
    vec3 position = vec3(dc_i5(aData.x, 10), dc_i5(aData.x, 5), dc_i5(aData.x, 0));
    gl_Position = vec4(position, 1.0) * model * view * projection;
}
