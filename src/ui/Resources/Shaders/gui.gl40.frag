#version 400

in vec2 frag_uv;
in vec4 frag_color;

uniform sampler2D tex;

out vec4 out_frag_color;

uniform float uUseTexture = 0.0;

// todo: delete this shader file

void main(void)
{
    vec4 texColor = texture(tex, frag_uv);
    if (uUseTexture > 0.0)
    out_frag_color = texColor * frag_color;
    else
    out_frag_color = frag_color;
}
