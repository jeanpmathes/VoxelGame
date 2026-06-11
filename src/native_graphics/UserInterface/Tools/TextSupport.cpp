#include "stdafx.h"

ui::TextSupport::TextSupport(Renderer& renderer)
    : renderer(renderer)
{
}

ui::Text& ui::TextSupport::GetText(WCHAR const* textContent, UINT textLength, TextFormat& format)
{
    auto  text   = std::make_unique<Text>(renderer);
    Text& result = *text;

    Text::Index const index = texts.Push(std::move(text));

    result.Reset(index, textContent, textLength, format);

    return result;
}

void ui::TextSupport::ReturnText(Text::Index const index)
{
    texts.Pop(index);
}

void ui::TextSupport::ValidateAllWrappedResourcesAreReturned() const
{
    if (texts.GetCount() > 0) renderer.GetClient().GetContext().GetDebugLayer().AddWarning(
                                                                                           std::format(
                                                                                                       "A total of {} wrapped texts have not been returned",
                                                                                                       texts.GetCount()).c_str());
}
