#version 430

out vec4 outputColor;

in vec3 normal;

flat in int texIndex;
in vec2 texCoord;

in vec4 tint;

layout(binding = 1) uniform sampler2DArray lowerArrayTexture;
layout(binding = 2) uniform sampler2DArray upperArrayTexture;

void main()
{
	vec4 color;

	if ((texIndex & 4096) == 0)
	{
		color = texture(lowerArrayTexture, vec3(texCoord.xy, (texIndex & 2047)));
	}
	else
	{
		color = texture(upperArrayTexture, vec3(texCoord.xy, (texIndex & 2047)));
	}
	
	if (color.a < 1)
	{
		discard;
	}

	float brightness = dot(normal, normalize(vec3(0.2, 0.7, 0.8)));
	brightness = (brightness == 0.0) ? 1.0 : max(brightness, 0.0) + 0.2;

	outputColor = color * tint * brightness;
}