#version 460

out vec4 outputColor;

in vec2 texCoord;

in float height;

uniform int textureId;
uniform sampler2DArray sampler;

uniform int mode;

uniform float upperBound;
uniform float lowerBound;

uniform vec4 tint;

#pragma include("color")

void main()
{
    vec4 color = texture(sampler, vec3(texCoord, textureId));

    const int MODE_BLOCK = 0;
    const int MODE_FLUID = 1;

    switch (mode)
    {
        case MODE_BLOCK:
        outputColor = color_select(color, 1.0, tint);
        break;

        case MODE_FLUID:
        outputColor = color;
        break;

        default :
        outputColor = vec4(1.0, 0.0, 0.0, 1.0);
        break;
    }

    if (height > upperBound || height < lowerBound)
    {
        discard;
    }
}
