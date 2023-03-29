#version 400

layout(location=0) in vec2 in_screen_coords;
layout(location=1) in vec2 in_uv;
layout(location=2) in vec4 in_color;

out vec2 frag_uv;
out vec4 frag_color;

uniform vec2 uScreenSize = vec2(1280, 720);

// todo: delete this shader file

void main(void)
{
    frag_uv = in_uv;
    frag_color = in_color;

    vec2 ndc_position = 2.0 * (in_screen_coords / uScreenSize) - 1.0;
    ndc_position.y *= -1.0;

    gl_Position = vec4(ndc_position, 0.0, 1);
}
