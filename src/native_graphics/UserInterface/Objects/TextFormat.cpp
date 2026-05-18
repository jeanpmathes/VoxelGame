#include "stdafx.h"

ui::TextFormat::TextFormat(Renderer& renderer, Index index, TextFormatDescription description, ComPtr<IDWriteTextFormat> format)
    : Object(renderer.GetClient())
  , renderer(&renderer)
  , index(index)
  , wrapped(std::move(format))
{
    // todo: Create text-format state from description.
    // Contract: do not store TextFormatDescription because its fontFamily pointer is local-only.
    (void)description;
}

void ui::TextFormat::Reset(Index newIndex, TextFormatDescription description, ComPtr<IDWriteTextFormat> format)
{
    // todo: Reset a reusable text format.
    // Contract: assign the new active index and wrapped format; do not store the local-only description.
    index   = newIndex;
    wrapped = std::move(format);
    (void)description;
}

void ui::TextFormat::Return()
{
    // todo: Return this text format through TextFormatSupport::Return.
    // Contract: remove it from the active bag, clear its index, and pool only while reusable and under the free-list cap.
}

void ui::TextFormat::SetIndex(std::optional<Index> newIndex) { index = newIndex; }

ui::Renderer& ui::TextFormat::GetRenderer() const { return *renderer; }

std::optional<ui::TextFormat::Index> ui::TextFormat::GetIndex() const { return index; }

IDWriteTextFormat* ui::TextFormat::GetWrapped() const { return wrapped.Get(); }
