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
        // ui::FontStretch is zero-based, while DWRITE_FONT_STRETCH is one-based.
        return static_cast<DWRITE_FONT_STRETCH>(static_cast<int>(stretch) + 1);
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

    void ConfigureTrimming(DWRITE_TRIMMING* trimming, ui::TextTrimming const trimmingType)
    {
        trimming->delimiter      = 0;
        trimming->delimiterCount = 0;

        switch (trimmingType)
        {
        case ui::TextTrimming::NONE:
            trimming->granularity = DWRITE_TRIMMING_GRANULARITY_NONE;
            break;
        case ui::TextTrimming::CHARACTER:
        case ui::TextTrimming::CHARACTER_ELLIPSIS:
            trimming->granularity = DWRITE_TRIMMING_GRANULARITY_CHARACTER;
            break;
        case ui::TextTrimming::WORD:
        case ui::TextTrimming::WORD_ELLIPSIS:
            trimming->granularity = DWRITE_TRIMMING_GRANULARITY_WORD;
            break;
        case ui::TextTrimming::PATH_ELLIPSIS:
            trimming->granularity = DWRITE_TRIMMING_GRANULARITY_CHARACTER;
            trimming->delimiter      = '\\';
            trimming->delimiterCount = 2;
            break;

        default:
            throw NativeException("Trimming not implemented.");
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
    Require(newDescription.fontFamily != nullptr);
    Require(std::isfinite(newDescription.size));
    Require(newDescription.size > 0.0f);
    Require(std::isfinite(newDescription.lineHeight));
    Require(newDescription.lineHeight >= 0.0f);

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

    ComPtr<IDWriteTextFormat2> textFormat2;
    TryDo(wrapped.As(&textFormat2));

    DWRITE_LINE_SPACING lineSpacing;
    lineSpacing.method           = DWRITE_LINE_SPACING_METHOD_PROPORTIONAL;
    lineSpacing.height           = newDescription.lineHeight;
    lineSpacing.baseline         = 1.0f;
    lineSpacing.leadingBefore    = 0.0f;
    lineSpacing.fontLineGapUsage = DWRITE_FONT_LINE_GAP_USAGE_DEFAULT;
    TryDo(textFormat2->SetLineSpacing(&lineSpacing));

    DWRITE_TRIMMING trimming;
    ConfigureTrimming(&trimming, newDescription.trimming);

    if (newDescription.trimming == TextTrimming::CHARACTER_ELLIPSIS || newDescription.trimming == TextTrimming::WORD_ELLIPSIS || newDescription.trimming ==
        TextTrimming::PATH_ELLIPSIS)
        TryDo(GetRenderer().GetContext().GetDirectWriteFactory()->CreateEllipsisTrimmingSign(wrapped.Get(), &trimmingSign));

    TryDo(wrapped->SetTrimming(&trimming, trimmingSign.Get()));
}

ui::Renderer& ui::TextFormat::GetRenderer() const { return *renderer; }

IDWriteTextFormat* ui::TextFormat::GetWrapped() const { return wrapped.Get(); }
