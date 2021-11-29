#version 430

in vec3 aPosition;
in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 model;
uniform mat4 projection;

void main()
{
	texCoord = aTexCoord;
	gl_Position = vec4(aPosition, 1.0) * model * projection;
}
