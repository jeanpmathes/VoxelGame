#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform vec3 color;
uniform sampler2D tex;

void main()
{
	vec4 texColor = texture(tex, texCoord);

	if (texColor.a == 0.0)
	{
		discard;
	}

	outputColor = vec4(color, 1.0) * texColor;
}