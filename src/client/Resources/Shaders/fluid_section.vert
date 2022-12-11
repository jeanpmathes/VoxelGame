#version 430

layout (location = 0) in ivec2 aData;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;
out vec3 normal;

out vec3 viewNormal;
out vec3 viewPosition;

uniform mat4 mv_matrix;
uniform mat4 mvp_matrix;

#pragma include("decode")

void main()
{
    int direction = !dc_bool(aData.y, 11) ? 1 : -1;

    int level = dc_i3(aData.y, 8);
    int sideHeight = dc_i4(aData.y, 12) - 1;

    float upperBound = ((direction > 0) ? (level + 1) : (7 - sideHeight)) * 0.125;
    float lowerBound = ((direction > 0) ? (sideHeight + 1) : (7 - level)) * 0.125;

    int n = dc_i3(aData.y, 16);
    normal = dc_sideToNormal(n);

    texIndex = dc_fluidTexIndex(aData.y);
    texCoord = dc_texCoord(aData.x, 30);

    tint = dc_tint(aData.y, 23);

    int end = dc_i1(aData.x, 9);
    vec3 position = vec3(dc_i5(aData.x, 10), dc_i4(aData.x, 5), dc_i5(aData.x, 0));

    if (n == 4)// Side: Bottom
    {
        position.y += (direction < 0) ? lowerBound : 0;
    }
    else if (n == 5)// Side: Top
    {
        position.y += (direction > 0) ? upperBound : 1;
    }
    else // Side: Front, Back, Left, Right
    {
        position.y += (end == 0) ? lowerBound : upperBound;
        texCoord.y = (end == 0) ? lowerBound : upperBound;
    }

    // Texture Repetition
    texCoord.x *= dc_i4(aData.x, 24) + 1;
    texCoord.y *= dc_i4(aData.x, 20) + 1;

    // Position and normal in view space
    viewNormal = (vec4(normal, 0.0) * mv_matrix).xyz;
    viewPosition = (vec4(position, 1.0) * mv_matrix).xyz;

    float distance = length(viewPosition);
    float offset = clamp(distance * 0.0001, 0.0, 0.1);

    gl_Position = vec4(position - (normal * offset), 1.0) * mvp_matrix;
}
