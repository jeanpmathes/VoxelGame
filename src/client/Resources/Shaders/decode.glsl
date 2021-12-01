/*
 * Utility functions for decoding data from integers.
 */

int dc_i1(int data, int bit)
{
    return (data >> bit) & 1;
}

int dc_i2(int data, int bit)
{
    return (data >> bit) & 3;
}

int dc_i3(int data, int bit)
{
    return (data >> bit) & 7;
}

int dc_i4(int data, int bit)
{
    return (data >> bit) & 15;
}

int dc_i5(int data, int bit)
{
    return (data >> bit) & 31;
}

bool dc_bool(int data, int bit)
{
    return dc_i1(data, bit) == 1;
}

int dc_texIndex(int data)
{
    return data & 8191;
}

int dc_liquidTexIndex(int data)
{
    return (((data & 127) - 1) << 4) + 1;
}

vec2 dc_texCoord(int data, int bit)
{
    return vec2((data >> (bit + 1)) & 1, (data >> bit) & 1);
}

vec4 dc_tint(int data, int bit)
{
    return vec4(dc_i3(data, bit + 6) / 7.0, dc_i3(data, bit + 3) / 7.0, dc_i3(data, bit) / 7.0, 1.0);
}

vec3 dc_normal(int data, int bit)
{
    int nx = dc_i5(data, bit + 10);
    int ny = dc_i5(data, bit +  5);
    int nz = dc_i5(data, bit +  0);

    vec3 normal = vec3((nx < 16) ? nx : (nx & 15) * -1, (ny < 16) ? ny : (ny & 15) * -1, (nz < 16) ? nz : (nz & 15) * -1);
    normal /= 15.0;
    normal = normalize(normal);
    normal = (isnan(normal.x) || isnan(normal.y) || isnan(normal.z)) ? vec3(0.0, 0.0, 0.0) : normal;

    return normal;
}

vec3 dc_sideToNormal(int side)
{
    vec3 normal = vec3(0.0, 0.0, 0.0);
    normal[((side >> 1) + 3 & 2) | (side >> 2)] = -1.0 + (2 * (side & 1));
    normal.z *= -1.0;
    normal = normalize(normal);

    return normal;
}
