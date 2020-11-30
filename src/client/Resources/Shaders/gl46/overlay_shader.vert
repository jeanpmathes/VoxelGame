#version 460

in vec3 aPosition;
in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 projection;

void main()
{
	texCoord = aTexCoord;

	gl_Position = vec4(aPosition, 1.0) * projection;
}