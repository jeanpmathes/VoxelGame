#version 330

in vec3 aPosition;
in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;
flat out int anim;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // Normal
    int nx = (aData.x >> 27) & 31;
    int ny = (aData.x >> 22) & 31;
    int nz = (aData.x >> 17) & 31;
    normal = vec3((nx < 16) ? nx : (nx & 15) * -1, (ny < 16) ? ny : (ny & 15) * -1, (nz < 16) ? nz : (nz & 15) * -1);
    normal /= 15.0;
    normal = normalize(normal);
    normal = (isnan(normal.x) || isnan(normal.y) || isnan(normal.z)) ? vec3(0.0, 0.0, 0.0) : normal;

    // Texture Index
    texIndex = aData.y & 8191;

    // Texture Coordinate
    texCoord = vec2(((aData.x >> 5) & 31) / 16.0, (aData.x & 31) / 16.0);

    // Tint
    tint = vec4(((aData.y >> 29) & 7) / 7.0, ((aData.y >> 26) & 7) / 7.0, ((aData.y >> 23) & 7) / 7.0, 1.0);

    // Animation
    anim = (aData.y >> 16) & 1;

    // Position
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}