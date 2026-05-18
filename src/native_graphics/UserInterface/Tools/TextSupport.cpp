#include "stdafx.h"

ui::TextSupport::TextSupport(Renderer& renderer)
    : renderer(renderer)
{
}

ui::Text& ui::TextSupport::Get(WCHAR const* text, TextFormat& format)
{
    // todo: Create a new UI text object.
    // Contract: create a new ui::Text, insert it into texts, and assign its active index; text layouts are not pooled.
    (void)text;
    (void)format;
    throw NativeException("TODO: create UI text.");
}

void ui::TextSupport::Return(Text& text)
{
    // todo: Return and destroy a UI text object.
    // Contract: remove it from texts and destroy it; Text objects are never stored in a free list.
    (void)text;
}
