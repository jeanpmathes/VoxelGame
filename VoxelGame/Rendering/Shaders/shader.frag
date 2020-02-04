#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
    vec4 color = texture(texture0, texCoord);
	
	if (color.a < 1)
	{
		discard;
	}

	outputColor = color;
}