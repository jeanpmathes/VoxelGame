#include "stdafx.h"

ui::Brush::Brush(Renderer& renderer, Index index, ComPtr<ID2D1SolidColorBrush> brush)
    : Object(renderer.GetClient())
  , renderer(&renderer)
  , index(index)
  , wrapped(std::move(brush))
{
}

void ui::Brush::Reset(Index newIndex, ComPtr<ID2D1SolidColorBrush> newBrush)
{
    // todo: Reset a reusable solid-color brush.
    // Contract: assign the new active index and wrapped brush supplied by BrushSupport.
    index   = newIndex;
    wrapped = std::move(newBrush);
}

void ui::Brush::Return()
{
    // todo: Return this brush through BrushSupport::ReturnSolidColorBrush.
    // Contract: remove it from the active bag, clear its active index, and pool only while the free list has space.
}

void ui::Brush::SetIndex(std::optional<Index> newIndex) { index = newIndex; }

ui::Renderer& ui::Brush::GetRenderer() const { return *renderer; }

std::optional<ui::Brush::Index> ui::Brush::GetIndex() const { return index; }

ID2D1Brush* ui::Brush::GetWrapped() const { return wrapped.Get(); }
