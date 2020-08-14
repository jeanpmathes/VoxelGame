#version 430

out vec4 outputColor;

in vec2 texCoord;
in vec3 normal;

flat in int texInd;

layout(binding = 5) uniform sampler2DArray firstArrayTexture;

void main()
{
	if (mod(texCoord.x, 0.125) > 0.0625) discard;

	vec4 color = texture(firstArrayTexture, vec3(texCoord, texInd));

	float brightness = clamp((dot(normal, normalize(vec3(0.3, 0.8, 0.5))) + 1.7) / 2.5, 0.0, 1.0);
	brightness = (length(normal) < 0.1) ? 1.0 : brightness;

	color *= brightness;

	outputColor = color;
}