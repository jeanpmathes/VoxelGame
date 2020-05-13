#version 430

in vec3 aPosition;
in int aTexIndex;
in vec2 aTexCoord;

flat out int texIndex;
out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    texIndex = aTexIndex;
    texCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}