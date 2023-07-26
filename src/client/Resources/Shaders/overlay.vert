#version 460

in vec3 aPosition;
in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 projection;

out float height;

void main()
{
    texCoord = aTexCoord;
    vec4 position = vec4(aPosition, 1.0) * projection;
    height = (position.y + 1) * 0.5;

    gl_Position = position;
}
