﻿#version 430

in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    int direction = (((aData.y >> 11) & 1) == 0) ? 1 : -1;

    int level = (aData.y >> 8) & 7;
    int sideHeight = ((aData.y >> 12) & 15) - 1;

    float upperBound = ((direction > 0) ? (level + 1) : (7 - sideHeight)) * 0.125;
    float lowerBound = ((direction > 0) ? (sideHeight + 1) : (7 - level)) * 0.125;

	// Normal
    int n = (aData.y >> 16) & 7;
    normal = vec3(0.0, 0.0, 0.0);
    normal[((n >> 1) + 3 & 2) | (n >> 2)] = -1.0 + (2 * (n & 1));
    normal.z *= -1.0;
    normal = normalize(normal);

    // Texture Index
    texIndex = aData.y & 127;

    // Texture Coordinate
    texCoord = vec2((aData.x >> 31) & 1, (aData.x >> 30) & 1);

    // Position and Texture
    int end = (aData.x >> 11) & 1;
    vec3 position = new vec3((aData.x >> 12) & 63, (aData.x >> 6) & 31, aData.x & 63);

    if (n == 4) // Side: Bottom
    {
        position.y += (direction < 0) ? lowerBound : 0;
    }
    else if (n == 5) // Side: Top
    {
        position.y += (direction > 0) ? upperBound : 1;
    }
    else // Side: Front, Back, Left, Right
    {
        position.y += (end == 0) ? lowerBound : upperBound;
        texCoord.y = (end == 0) ? lowerBound : upperBound;
    }

	gl_Position = vec4(position, 1.0) * model * view * projection;
}