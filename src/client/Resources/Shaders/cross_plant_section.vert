#version 430

in ivec2 aData;

out vec3 normal;

flat out int texIndex;
out vec2 texCoord;

out vec4 tint;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float time;

/*
 * Array and textureless GLSL 2D simplex noise function.
 * Ian McEwan, Ashima Arts.
 * 
 * Copyright (C) 2011 Ashima Arts. All rights reserved.
 * Distributed under the MIT License.
 * 
 * https://github.com/ashima/webgl-noise
 * https://github.com/stegu/webgl-noise
 */

vec3 mod289(vec3 x) 
{
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec2 mod289(vec2 x) 
{
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec3 permute(vec3 x) 
{
  return mod289(((x*34.0)+10.0)*x);
}

float noise(vec2 v)
{
    const vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                        0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                       -0.577350269189626,  // -1.0 + 2.0 * C.x
                        0.024390243902439); // 1.0 / 41.0
    // First corner
    vec2 i  = floor(v + dot(v, C.yy) );
    vec2 x0 = v -   i + dot(i, C.xx);

    // Other corners
    vec2 i1;
    i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;

    // Permutations
    i = mod289(i); // Avoid truncation effects in permutation
    vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))
		+ i.x + vec3(0.0, i1.x, 1.0 ));

    vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
    m = m*m ;
    m = m*m ;

    // Gradients: 41 points uniformly over a line, mapped onto a diamond.
    // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

    vec3 x = 2.0 * fract(p * C.www) - 1.0;
    vec3 h = abs(x) - 0.5;
    vec3 ox = floor(x + 0.5);
    vec3 a0 = x - ox;

    // Normalise gradients implicitly by scaling m
    // Approximation of: m *= inversesqrt( a0*a0 + h*h );
    m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

    // Compute final noise value at P
    vec3 g;
    g.x  = a0.x  * x0.x  + h.x  * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

/*
 * End of "Array and textureless GLSL 2D simplex noise function."
 */

void main()
{
    // Normal.
    normal = vec3(0, 0, 0);

    // Texture Index
    texIndex = aData.y & 8191;

    // Texture Coordinate
    int u = (aData.x >> 31) & 1;
    int v = (aData.x >> 30) & 1;
    texCoord = vec2(u, v);

    // Tint
    tint = vec4(((aData.y >> 29) & 7) / 7.0, ((aData.y >> 26) & 7) / 7.0, ((aData.y >> 23) & 7) / 7.0, 1.0);

    // Cross plant information.
    bool isUpper = ((aData.y >> 20) & 1) == 1;
    bool isLowered = ((aData.y >> 21) & 1) == 1;
    bool hasUpper = ((aData.y >> 22) & 1) == 1;

    // Position
    vec3 position = vec3((aData.x >> 12) & 63, (aData.x >> 6) & 63, aData.x & 63);
    int orientation = (aData.x >> 28) & 1;

    float xOffset = (u == 0 ? +1 : -1) * 0.145;
    float zOffset = (u == 0 ? -1 : +1) * 0.145;
    if (orientation == 1) zOffset = xOffset;

    position.x += xOffset;
    position.z += zOffset;

    if (isLowered) position.y -= 0.0625;

    // Sway in wind.
    const float swayAmplitude = 0.1;
    const float swaySpeed = 0.8;

    vec3 wind = vec3(0.7, 0, 0.7);
    float swayStrength = texCoord.y;
    if (hasUpper) swayStrength = (swayStrength + (isUpper ? 1.0 : 0.0)) / 2.0;

    position += wind * noise(vec2(position.xz + wind.xz * time * swaySpeed)) * swayAmplitude * swayStrength;

    gl_Position = vec4(position, 1.0) * model * view * projection;
}