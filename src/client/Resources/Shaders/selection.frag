#version 430

out vec4 outputColor;

uniform vec3 color;

void main()
{
    outputColor = vec4(color, 1f);
}
