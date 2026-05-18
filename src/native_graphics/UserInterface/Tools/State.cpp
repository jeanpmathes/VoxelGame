#include "stdafx.h"

void ui::State::PushOffset(PointF offset)
{
    // todo: Push offset and update the Direct2D transform.
    // Contract: offsets are additive and reflected by GetCurrentTransform.
    (void)offset;
}

void ui::State::PopOffset()
{
    // todo: Pop offset and update the Direct2D transform.
    // Contract: report underflow through the DirectX info queue in NATIVE_DEBUG.
}

void ui::State::PushClip(RectangleF rectangle)
{
    // todo: Intersect with the current clip and call PushAxisAlignedClip immediately.
    // Contract: clipping is active whenever clipStack is non-empty; there is no begin/end clip state.
    (void)rectangle;
}

void ui::State::PopClip()
{
    // todo: Call PopAxisAlignedClip and remove the current clip.
    // Contract: report underflow through the DirectX info queue in NATIVE_DEBUG.
}

void ui::State::PushOpacity(FLOAT opacity)
{
    // todo: Multiply with current opacity and push a Direct2D layer.
    // Contract: opacity must be in [0, 1], and validation reports out-of-range values in NATIVE_DEBUG.
    (void)opacity;
}

void ui::State::PopOpacity()
{
    // todo: Pop the Direct2D layer and restore the previous opacity.
    // Contract: report underflow through the DirectX info queue in NATIVE_DEBUG.
}

bool ui::State::IsCurrentClipEmpty() const
{
    // todo: Return whether the currently intersected clip rejects all drawing.
    return false;
}

D2D1_MATRIX_3X2_F ui::State::GetCurrentTransform() const
{
    // todo: Return the transform for the current accumulated offset.
    return D2D1::Matrix3x2F::Translation(currentOffset.x, currentOffset.y);
}

ui::RectangleF ui::State::GetCurrentClip() const
{
    return currentClip;
}

FLOAT ui::State::GetCurrentOpacity() const { return currentOpacity; }

void ui::State::Validate() const
{
    // todo: Report non-empty offset, clip, and opacity stacks through the DirectX info queue in NATIVE_DEBUG.
}
