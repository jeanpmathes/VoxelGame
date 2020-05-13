#version 430

out vec4 outputColor;

flat in int texIndex;
in vec2 texCoord;

layout(binding = 1) uniform sampler2DArray lowerArrayTexture;
layout(binding = 2) uniform sampler2DArray upperArrayTexture;

void main()
{
	vec4 color;

	if ((texIndex & 4096) == 0)
	{
		// Use lowerTextureArray
		color = texture(lowerArrayTexture, vec3(texCoord.xy, (texIndex & 2047)));
	}
	else
	{
		// Use upperTextureArray
		color = texture(upperArrayTexture, vec3(texCoord.xy, (texIndex & 2047)));
	}
	
	if (color.a < 1)
	{
		discard;
	}

	outputColor = color;
}