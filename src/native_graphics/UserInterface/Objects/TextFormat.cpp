#include "stdafx.h"

namespace
{
    DWRITE_FONT_WEIGHT GetFontWeight(INT16 const weight)
    {
        return static_cast<DWRITE_FONT_WEIGHT>(weight);
    }

    DWRITE_FONT_STYLE GetFontStyle(ui::FontStyle const style)
    {
        switch (style)
        {
        case ui::FontStyle::NORMAL:
            return DWRITE_FONT_STYLE_NORMAL;
        case ui::FontStyle::ITALIC:
            return DWRITE_FONT_STYLE_ITALIC;
        case ui::FontStyle::OBLIQUE:
            return DWRITE_FONT_STYLE_OBLIQUE;

        default:
            throw NativeException("FontStyle not implemented.");
        }
    }

    DWRITE_FONT_STRETCH GetFontStretch(ui::FontStretch const stretch)
    {
        return static_cast<DWRITE_FONT_STRETCH>(stretch);
    }

    DWRITE_WORD_WRAPPING GetTextWrapping(ui::TextWrapping const wrapping)
    {
        switch (wrapping)
        {
        case ui::TextWrapping::NO_WRAP:
            return DWRITE_WORD_WRAPPING_NO_WRAP;
        case ui::TextWrapping::WRAP:
            return DWRITE_WORD_WRAPPING_WRAP;

        default:
            throw NativeException("TextWrapping not implemented.");
        }
    }

    DWRITE_TEXT_ALIGNMENT GetTextAlignment(ui::TextAlignment const alignment)
    {
        switch (alignment)
        {
        case ui::TextAlignment::LEADING:
            return DWRITE_TEXT_ALIGNMENT_LEADING;
        case ui::TextAlignment::CENTER:
            return DWRITE_TEXT_ALIGNMENT_CENTER;
        case ui::TextAlignment::TRAILING:
            return DWRITE_TEXT_ALIGNMENT_TRAILING;
        case ui::TextAlignment::JUSTIFY:
            return DWRITE_TEXT_ALIGNMENT_JUSTIFIED;

        default:
            throw NativeException("TextAlignment not implemented.");
        }
    }
}

ui::TextFormat::TextFormat(Renderer& renderer)
    : Object(renderer.GetClient())
  , renderer(&renderer)
{
}

void ui::TextFormat::Return()
{
    Index const oldIndex = index.value();
    index                = std::nullopt;

    renderer->GetTextFormatSupport().ReturnTextFormat(oldIndex);
}

void ui::TextFormat::Reset(Index newIndex, TextFormatDescription const& newDescription)
{
    index = newIndex;

    TryDo(
          renderer->GetContext().GetDirectWriteFactory()->CreateTextFormat(
                                                                           newDescription.fontFamily,
                                                                           nullptr,
                                                                           GetFontWeight(newDescription.weight),
                                                                           GetFontStyle(newDescription.style),
                                                                           GetFontStretch(newDescription.stretch),
                                                                           newDescription.size,
                                                                           GetRenderer().GetClient().GetApplicationLocale(),
                                                                           &wrapped));

    TryDo(wrapped->SetWordWrapping(GetTextWrapping(newDescription.wrapping)));
    TryDo(wrapped->SetTextAlignment(GetTextAlignment(newDescription.alignment)));

    // TryDo(wrapped->SetTrimming()); // todo: implement correct trimming

    // TryDo(wrapped->SetLineSpacing()) // todo: implement line spacing
}

ui::Renderer& ui::TextFormat::GetRenderer() const { return *renderer; }

IDWriteTextFormat* ui::TextFormat::GetWrapped() const { return wrapped.Get(); }
