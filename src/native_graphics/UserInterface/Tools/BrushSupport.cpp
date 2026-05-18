#include "stdafx.h"

ui::BrushSupport::BrushSupport(Renderer& renderer)
    : renderer(renderer)
{
}

ui::Brush& ui::BrushSupport::GetSolidColorBrush(ColorF color)
{
    // todo: Create or reuse a solid-color ui::Brush.
    // Contract: first reuse freeSolidColorBrushes, otherwise allocate; insert into brushes, set the active index,
    // and return the active brush by reference.
    (void)color;
    throw NativeException("TODO: create UI solid-color brush.");
}

void ui::BrushSupport::ReturnSolidColorBrush(Brush& brush, ComPtr<ID2D1SolidColorBrush> wrapped)
{
    // todo: Return a solid-color brush to the free list.
    // Contract: remove it from brushes, clear its active index, and store it only while below MAX_FREE_SOLID_COLOR_BRUSHES.
    (void)brush;
    (void)wrapped;
}

ID2D1Brush* ui::BrushSupport::GetScratchSolidColorBrush(ColorF color)
{
    // todo: Update and return the reusable scratch solid-color brush used by direct color commands.
    // Contract: do not allocate a new brush per draw call and do not create a ui::Brush object.
    (void)color;
    return scratchSolidColorBrush.Get();
}
