#version 430

out vec4 outputColor;

in vec3 normal;

flat in int texIndex;
in vec2 texCoord;
flat in ivec2 texCordMax;

in vec4 tint;
flat in int anim;

layout(binding = 1) uniform sampler2DArray firstArrayTexture;
layout(binding = 2) uniform sampler2DArray secondArrayTexture;
layout(binding = 3) uniform sampler2DArray thirdArrayTexture;
layout(binding = 4) uniform sampler2DArray fourthArrayTexture;

uniform float time;

#pragma include("color")

void main()
{
	vec4 color;

	float quadID = -mod(gl_PrimitiveID, 2) + gl_PrimitiveID;
	int animatedTexOffset = texIndex + int(mod(anim * time * 8 + anim * quadID * 0.125, 8));

	vec2 clampLimit = (texCordMax == ivec2(0)) ? vec2(1.0) : vec2(texCordMax);
	vec2 clampedTexCoord = clamp(texCoord, vec2(0, 0), clampLimit);

	if ((animatedTexOffset & 8192) == 0)
	{
		if ((animatedTexOffset & 4096) == 0)
		{
			color = texture(firstArrayTexture, vec3(clampedTexCoord.xy, (animatedTexOffset & 2047)));
		}
		else
		{
			color = texture(secondArrayTexture, vec3(clampedTexCoord.xy, (animatedTexOffset & 2047)));
		}
	}
	else
	{
		if ((animatedTexOffset & 4096) == 0)
		{
			color = texture(thirdArrayTexture, vec3(clampedTexCoord.xy, (animatedTexOffset & 2047)));
		}
		else
		{
			color = texture(fourthArrayTexture, vec3(clampedTexCoord.xy, (animatedTexOffset & 2047)));
		}
	}

	float brightness = clamp((dot(normal, normalize(vec3(0.3, 0.8, 0.5))) + 1.7) / 2.5, 0.0, 1.0);
	brightness = (length(normal) < 0.1) ? 1.0 : brightness;

	outputColor = color_select(color, brightness, tint);
}
