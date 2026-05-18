#include "stdafx.h"

ui::Text::Text(Renderer& renderer, Index index, WCHAR const* text, TextFormat& format)
    : Object(renderer.GetClient())
  , renderer(&renderer)
  , index(index)
  , format(&format)
{
    // todo: Create the IDWriteTextLayout for the supplied text and text format.
    // Contract: Text objects are not pooled, and there is no mutating C# API for Text or TextFormat.
    (void)text;
}

void ui::Text::Return()
{
    // todo: Return this text through TextSupport::Return.
    // Contract: remove it from the active bag and destroy it; text layouts are not pooled.
}

void ui::Text::SetIndex(std::optional<Index> newIndex) { index = newIndex; }

ui::Renderer& ui::Text::GetRenderer() const { return *renderer; }

std::optional<ui::Text::Index> ui::Text::GetIndex() const { return index; }

ui::TextFormat& ui::Text::GetFormat() const { return *format; }

IDWriteTextLayout* ui::Text::GetLayout() { return layout.Get(); }

ui::SizeF ui::Text::Measure(SizeF newAvailableSize)
{
    // todo: Measure the DirectWrite layout.
    // Contract: set availableSize, update the layout maximum width and height, and return measured UI coordinates
    // supplied by C#; GetLayout returns the layout created by the last Measure call.
    availableSize = newAvailableSize;
    return {0.0f, 0.0f};
}
