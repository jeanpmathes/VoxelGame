#version 430

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D tex;

void main()
{
	outputColor = texture(tex, texCoord);

	if (outputColor.a == 0.0)
	{
		discard;
	}
}