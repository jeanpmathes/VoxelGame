#include "stdafx.h"

ui::TextFormatSupport::TextFormatSupport(Renderer& renderer)
    : renderer(renderer)
{
}

ui::TextFormat& ui::TextFormatSupport::Get(TextFormatDescription description)
{
    // todo: Create or reuse a UI text format.
    // Contract: first reuse freeFormats, otherwise allocate; insert into formats and set the active index.
    (void)description;
    throw NativeException("TODO: create UI text format.");
}

void ui::TextFormatSupport::Return(TextFormat& format)
{
    // todo: Return a UI text format to the free list.
    // Contract: remove it from formats, clear its active index, and pool it when reusable and below MAX_FREE_TEXT_FORMATS.
    (void)format;
}
