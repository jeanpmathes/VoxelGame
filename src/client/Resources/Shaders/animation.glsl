int animation_offset(int index, float time, int frame_count, float quad_factor)
{
    float quadID = -mod(gl_PrimitiveID, 2) + gl_PrimitiveID;
    return index + int(mod(time * frame_count + quadID * quad_factor, frame_count));
}

int animation_block(int index, float time)
{
    return animation_offset(index, time, 8, 0.125);
}

int animation_fluid(int index, float time)
{
    return animation_offset(index, time, 16, 0.00);
}
