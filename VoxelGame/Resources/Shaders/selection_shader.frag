#version 430 core

out vec4 outputColor;

uniform vec3 color;

void main()
{
	outputColor = vec4(color, 1f);
	gl_FragDepth = 0;
}