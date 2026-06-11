#include "stdafx.h"

ui::TextFormatSupport::TextFormatSupport(Renderer& renderer)
    : renderer(renderer)
{
}

ui::TextFormat& ui::TextFormatSupport::GetTextFormat(TextFormatDescription const& description)
{
    auto        textFormat = std::make_unique<TextFormat>(renderer);
    TextFormat& result     = *textFormat;

    TextFormat::Index const index = textFormats.Push(std::move(textFormat));

    result.Reset(index, description);

    return result;
}

void ui::TextFormatSupport::ReturnTextFormat(TextFormat::Index const index)
{
    textFormats.Pop(index);
}

void ui::TextFormatSupport::ValidateAllWrappedResourcesAreReturned() const
{
    if (textFormats.GetCount() > 0) renderer.GetClient().GetContext().GetDebugLayer().AddWarning(
                                                                                                 std::format(
                                                                                                             "A total of {} wrapped text formats have not been returned",
                                                                                                             textFormats.GetCount()).c_str());
}
