#version 430

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
    int height = (aData.y >> 16) & 15;
    float upperBound = (height + 1) * 0.0625;

	// Normal
    int n = (aData.y >> 20) & 7;
    vec3 normal = vec3(0.0, 0.0, 0.0);
    normal[((n >> 1) + 3 & 2) | (n >> 2)] = -1.0 + (2 * (n & 1));
    normal.z *= -1.0;
    normal = normalize(normal);

    // Texture Index
    texIndex = aData.y & 8191;

    // Texture Coordinate
    texCoord = vec2((aData.x >> 31) & 1, (aData.x >> 30) & 1);

    // Tint
    tint = vec4(((aData.y >> 29) & 7) / 7.0, ((aData.y >> 26) & 7) / 7.0, ((aData.y >> 23) & 7) / 7.0, 1.0);

    // Position and Texture
    int end = (aData.x >> 11) & 1;
    vec3 position = vec3((aData.x >> 12) & 63, (aData.x >> 6) & 31, aData.x & 63);

    if (n == 4) // Side: Bottom
    {
        position.y += 0.0;
    }
    else if (n == 5) // Side: Top
    {
        position.y += upperBound;
    }
    else // Side: Front, Back, Left, Right
    {
        position.y += (end == 0) ? 0.0 : upperBound;
        texCoord.y = (end == 0) ? 0.0 : upperBound;
    }

    // Texture Repetition
    texCoord.x *= ((aData.x >> 25) & 31) + 1;
    texCoord.y *= ((aData.x >> 20) & 31) + 1;

	gl_Position = vec4(position, 1.0) * model * view * projection;
}