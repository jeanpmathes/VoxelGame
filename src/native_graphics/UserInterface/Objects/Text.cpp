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

    layout = nullptr;
    format = nullptr;

    text.clear();
    availableSize = {};

    renderer->GetTextSupport().ReturnText(oldIndex);
}

void ui::Text::Reset(Index const newIndex, WCHAR const* newText, UINT const newTextLength, TextFormatDescription const& newFormat)
{
    index = newIndex;

    text.assign(newText, newTextLength);
    format        = &renderer->GetTextFormatSupport().GetTextFormat(newFormat);
    availableSize = {.width = 0.0f, .height = 0.0f};

    InitializeLayout();
}

void ui::Text::SetContent(WCHAR const* newText, UINT const newTextLength)
{
    text.assign(newText, newTextLength);

    InitializeLayout();
}

void ui::Text::SetFormat(TextFormatDescription const& newFormat)
{
    format = &renderer->GetTextFormatSupport().GetTextFormat(newFormat);

    InitializeLayout();
}

void ui::Text::InitializeLayout()
{
    layout = nullptr;

    if (format == nullptr) return;

    TryDo(
          renderer->GetContext().GetDirectWriteFactory()->CreateTextLayout(
                                                                           text.c_str(),
                                                                           static_cast<UINT32>(text.length()),
                                                                           format->GetWrapped(),
                                                                           availableSize.width,
                                                                           availableSize.height,
                                                                           &layout));
}

ui::Renderer& ui::Text::GetRenderer() const { return *renderer; }

std::optional<ui::Text::Index> ui::Text::GetIndex() const { return index; }

IDWriteTextLayout* ui::Text::GetWrapped() const { return layout.Get(); }

ui::Size ui::Text::Measure(Size const newAvailableSize)
{
    Require(newAvailableSize.width >= 0.0f);
    Require(newAvailableSize.height >= 0.0f);

    availableSize = newAvailableSize;

    if (layout == nullptr) return Size{.width = 0.0f, .height = 0.0f};

    TryDo(layout->SetMaxWidth(newAvailableSize.width));
    TryDo(layout->SetMaxHeight(newAvailableSize.height));

    DWRITE_TEXT_METRICS metrics;
    TryDo(layout->GetMetrics(&metrics));

    return Size{.width = metrics.width, .height = metrics.height};
}
