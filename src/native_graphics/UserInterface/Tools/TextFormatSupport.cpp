#include "stdafx.h"

ui::TextFormatSupport::TextFormatSupport(Renderer& renderer)
    : renderer(renderer)
{
}

ui::TextFormat& ui::TextFormatSupport::GetTextFormat(TextFormatDescription const& description)
{
    auto const it = formats.find(description);
    if (it != formats.end()) return *it->second;

    auto        textFormat = std::make_unique<TextFormat>(renderer);
    TextFormat& result     = *textFormat;

    result.Reset(description);

    TextFormatDescription storedDescription = description;
    if (description.fontFamily != nullptr)
    {
        auto const [familyIt, _]     = fontFamilies.insert(description.fontFamily);
        storedDescription.fontFamily = familyIt->c_str();
    }

    formats.emplace(storedDescription, std::move(textFormat));

    return result;
}
