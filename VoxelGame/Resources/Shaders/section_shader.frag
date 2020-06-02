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

	float brightness = clamp((dot(normal, normalize(vec3(0.3, 0.8, 0.5))) + 1.7) / 2.5, 0.0, 1.0);
	brightness = (length(normal) < 0.1) ? 1.0 : brightness;

	outputColor = color * tint * brightness;
}