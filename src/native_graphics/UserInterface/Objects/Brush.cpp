#include "stdafx.h"

ui::Brush::Brush(Renderer& renderer)
    : Object(renderer.GetClient())
  , renderer(&renderer)
{
}

void ui::Brush::Return()
{
    Index const oldIndex = index.value();
    index                = std::nullopt;

    renderer->GetBrushSupport().ReturnSolidColorBrush(oldIndex);
}

void ui::Brush::Reset(Index newIndex, ColorF const newColor)
{
    index = newIndex;

    if (wrapped == nullptr) TryDo(renderer->GetContext().GetDirect2DDeviceContext()->CreateSolidColorBrush(newColor.ToD2D1(), &wrapped));
    else wrapped->SetColor(newColor.ToD2D1());
}

ui::Renderer& ui::Brush::GetRenderer() const { return *renderer; }

ID2D1Brush* ui::Brush::GetWrapped() const { return wrapped.Get(); }
