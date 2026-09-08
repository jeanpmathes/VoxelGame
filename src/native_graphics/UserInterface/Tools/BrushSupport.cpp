#include "stdafx.h"

ui::BrushSupport::BrushSupport(Renderer& renderer)
    : renderer(renderer)
{
    D2D1_COLOR_F const black = D2D1::ColorF(0.0f, 0.0f, 0.0f, 1.0f);
    TryDo(renderer.GetContext().GetDirect2DDeviceContext()->CreateSolidColorBrush(black, &scratchSolidColorBrush));
}

ui::Brush& ui::BrushSupport::GetSolidColorBrush(Color const color)
{
    std::unique_ptr<Brush> brush;

    if (freeSolidColorBrushes.empty())
    {
        brush = std::make_unique<Brush>(renderer);
    }
    else
    {
        brush = std::move(freeSolidColorBrushes.back());
        freeSolidColorBrushes.pop_back();
    }

    Brush& result = *brush;

    auto const index = brushes.Push(std::move(brush));
    result.Reset(index, color);

    return result;
}

void ui::BrushSupport::ReturnSolidColorBrush(Brush::Index const index)
{
    std::unique_ptr<Brush> ptr = std::move(brushes.Pop(index));

    if (freeSolidColorBrushes.size() < MAX_FREE_BRUSHES) freeSolidColorBrushes.push_back(std::move(ptr));
}

// ReSharper disable once CppMemberFunctionMayBeConst
ID2D1Brush* ui::BrushSupport::UseRawSolidColorBrush(Color const color)
{
    scratchSolidColorBrush->SetColor(color.ToD2D1());

    return scratchSolidColorBrush.Get();
}

void ui::BrushSupport::ValidateAllWrappedResourcesAreReturned() const
{
    if (brushes.GetCount() > 0)
        renderer.GetClient().GetContext().GetDebugLayer().AddWarning(
                                                                     std::format(
                                                                                 "A total of {} wrapped brushes have not been returned",
                                                                                 brushes.GetCount()).c_str());
}
