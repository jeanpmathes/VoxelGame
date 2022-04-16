vec4 color_select(vec4 color, float factors, vec4 tint)
{
    if (color.a < 0.1)
    {
        discard;
    }

    vec4 result = color;

    result = (color.a < 0.3) ? color * factors : color * tint * factors;
    result.a = 1.0;

    return result;
}
