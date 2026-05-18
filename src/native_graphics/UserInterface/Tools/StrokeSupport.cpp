#include "stdafx.h"

ui::StrokeSupport::StrokeSupport(Renderer& renderer)
    : renderer(renderer)
{
}

ID2D1StrokeStyle* ui::StrokeSupport::Get(StrokeStyle style)
{
    // todo: Return the Direct2D stroke style matching the UI stroke-style preset.
    // Contract: maintain solid, dashes, squared, and dotted presets for ui::StrokeStyle mapping.
    (void)style;
    return solid.Get();
}
