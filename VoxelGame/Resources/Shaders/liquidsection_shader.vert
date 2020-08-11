#version 430

in vec3 aPosition;

in vec2 aTexCoord;
in vec3 aNormal;
in int aTexInd;

out vec2 texCoord;
out vec3 normal;
flat out int texInd;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	texCoord = aTexCoord;
	normal = aNormal;
	texInd = aTexInd;

	gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}