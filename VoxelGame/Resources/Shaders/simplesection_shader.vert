#version 430

in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // Normal
    int n = (aData.x >> 18) & 7;
    normal = vec3(0.0, 0.0, 0.0);
    normal[((n >> 1) + 3 & 2) | (n >> 2)] = -1.0 + (2 * (n & 1));
    normal.z *= -1.0;
    normal = normalize(normal);

    // Texture Index
    texIndex = aData.y & 4095;

    // Texture Coordinate
    texCoord = vec2((aData.y >> 18) & 1, (aData.y >> 17) & 1);

    // Tint
    tint = vec4(((aData.y >> 29) & 7) / 7.0, ((aData.y >> 26) & 7) / 7.0, ((aData.y >> 23) & 7) / 7.0, 1.0);

    // Position
    vec3 position = vec3((aData.x >> 12) & 63, (aData.x >> 6) & 63, aData.x & 63);
    gl_Position = vec4(position, 1.0) * model * view * projection;
}