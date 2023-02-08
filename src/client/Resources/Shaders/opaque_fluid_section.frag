#version 430

out vec4 outputColor;

flat in int texIndex;
in vec2 texCoord;

in vec4 tint;

layout(binding = 5) uniform sampler2DArray arrayTexture;

uniform float time;

#pragma include("animation")

void main()
{
	vec4 color = texture(arrayTexture, vec3(texCoord, animation_fluid(texIndex, time)));

	color *= tint;

	outputColor = color;
}
