#version 430

out vec4 outputColor;

in vec3 normal;

flat in int texIndex;
in vec2 texCoord;

in vec4 tint;
flat in int anim;

layout(binding = 1) uniform sampler2DArray firstArrayTexture;
layout(binding = 2) uniform sampler2DArray secondArrayTexture;
layout(binding = 1) uniform sampler2DArray thirdArrayTexture;
layout(binding = 2) uniform sampler2DArray fourthArrayTexture;

uniform float time;

void main()
{
	vec4 color;

	float quadID = -mod(gl_PrimitiveID, 2) + gl_PrimitiveID;
	int animatedTexOffset = texIndex + int(mod(anim * time * 8 + anim * quadID * 0.125, 8));

	if ((animatedTexOffset & 8192) == 0)
	{
		if ((animatedTexOffset & 4096) == 0)
		{
			color = texture(firstArrayTexture, vec3(texCoord.xy, (animatedTexOffset & 2047)));
		}
		else
		{
			color = texture(secondArrayTexture, vec3(texCoord.xy, (animatedTexOffset & 2047)));
		}
	}
	else
	{
		if ((animatedTexOffset & 4096) == 0)
		{
			color = texture(thirdArrayTexture, vec3(texCoord.xy, (animatedTexOffset & 2047)));
		}
		else
		{
			color = texture(fourthArrayTexture, vec3(texCoord.xy, (animatedTexOffset & 2047)));
		}
	}

	if (color.a < 0.1)
	{
		discard;
	}

	float brightness = clamp((dot(normal, normalize(vec3(0.3, 0.8, 0.5))) + 1.7) / 2.5, 0.0, 1.0);
	brightness = (length(normal) < 0.1) ? 1.0 : brightness;

	color = (color.a < 0.3) ? color * brightness : color * tint * brightness;
	color.a = 1.0;

	outputColor = color;
}