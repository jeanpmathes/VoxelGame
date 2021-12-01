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
    int height = dc_i4(aData.y, 16);
    float upperBound = (height + 1) * 0.0625;

    int n = dc_i3(aData.y, 20);
    normal = dc_sideToNormal(n);

    texIndex = dc_texIndex(aData.y);
    texCoord = dc_texCoord(aData.x, 30);

    tint = dc_tint(aData.y, 23);

    int end = dc_i1(aData.x, 9);
    vec3 position = vec3(dc_i5(aData.x, 10), dc_i4(aData.x, 5), dc_i5(aData.x, 0));

    if (n == 4)// Side: Bottom
    {
        position.y += 0.0;
    }
    else if (n == 5)// Side: Top
    {
        position.y += upperBound;
    }
    else // Side: Front, Back, Left, Right
    {
        position.y += (end == 0) ? 0.0 : upperBound;
        texCoord.y = (end == 0) ? 0.0 : upperBound;
    }

    // Texture Repetition
    int xLen = dc_i4(aData.x, 24) + 1;
    int yLen = dc_i4(aData.x, 20) + 1;

    texCordMax = ivec2(xLen, yLen);

    texCoord.x *= xLen;
    texCoord.y *= yLen;

    gl_Position = vec4(position, 1.0) * model * view * projection;
}
