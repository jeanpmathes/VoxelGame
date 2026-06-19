#include "stdafx.h"

ui::Text::Text(Renderer& renderer)
    : Object(renderer.GetClient())
  , renderer(&renderer)
{
}

void ui::Text::Return()
{
    Index const oldIndex = index.value();
    index                = std::nullopt;

    layout        = nullptr;
    format        = nullptr;
    availableSize = {};

    renderer->GetTextSupport().ReturnText(oldIndex);
}

void ui::Text::Reset(Index newIndex, WCHAR const* newText, UINT const newTextLength, TextFormat& newFormat)
{
    index = newIndex;

    format        = &newFormat;
    availableSize = {.width = 0.0f, .height = 0.0f};

    TryDo(renderer->GetContext().GetDirectWriteFactory()->CreateTextLayout(newText, newTextLength, newFormat.GetWrapped(), availableSize.width, availableSize.height, &layout));
}

ui::Renderer& ui::Text::GetRenderer() const { return *renderer; }

std::optional<ui::Text::Index> ui::Text::GetIndex() const { return index; }

ui::TextFormat& ui::Text::GetFormat() const { return *format; }

IDWriteTextLayout* ui::Text::GetWrapped() const { return layout.Get(); }

ui::SizeF ui::Text::Measure(SizeF const newAvailableSize)
{
    Require(newAvailableSize.width >= 0.0f);
    Require(newAvailableSize.height >= 0.0f);

    availableSize = newAvailableSize;

    TryDo(layout->SetMaxWidth(newAvailableSize.width));
    TryDo(layout->SetMaxHeight(newAvailableSize.height));

    DWRITE_TEXT_METRICS metrics;
    TryDo(layout->GetMetrics(&metrics));

    return SizeF{.width = metrics.width, .height = metrics.height};
}
