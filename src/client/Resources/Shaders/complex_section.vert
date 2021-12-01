#version 430

in vec3 aPosition;
in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;
flat out int anim;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

#pragma include("noise")
#pragma include("decode")

void main()
{
    normal = dc_normal(aData.x, 17);
    texIndex = dc_texIndex(aData.y);
    texCoord = vec2(dc_i5(aData.x, 5) / 16.0, dc_i5(aData.x, 0) / 16.0);
    tint = dc_tint(aData.y, 23);
    anim = dc_i1(aData.y, 16);

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
