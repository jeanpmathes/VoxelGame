#version 460

out vec4 outputColor;

in vec2 texCoord;

uniform int texId;
uniform sampler2DArray tex;

void main()
{
	outputColor = texture(tex, vec3(texCoord, texId));
}