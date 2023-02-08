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
uniform int isAnimated;

uniform float time;

#pragma include("color")
#pragma include("animation")

void main()
{
    const int BLOCK_MODE = 0;
    const int FLUID_MODE = 1;

    int animatedTextureId;

    switch (mode)
    {
        case BLOCK_MODE:
        animatedTextureId = animation_block(textureId, time);
        break;

        case FLUID_MODE:
        animatedTextureId = animation_fluid(textureId, time);
        break;

        default :
        animatedTextureId = textureId;
        break;
    }

    vec4 color = texture(sampler, vec3(texCoord, (isAnimated != 0) ? animatedTextureId : textureId));

    switch (mode)
    {
        case BLOCK_MODE:
        outputColor = color_select(color, 1.0, tint);
        break;

        case FLUID_MODE:
        outputColor = color * tint;
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
