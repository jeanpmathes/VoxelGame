#include "stdafx.h"

namespace
{
    constexpr D2D1_ANTIALIAS_MODE AntialiasMode = D2D1_ANTIALIAS_MODE_PER_PRIMITIVE;
}

ui::State::State(Renderer& renderer)
    : renderer(renderer)
{
}

void ui::State::PushOffset(PointF const offset)
{
    PointF previousOffset = {.x = 0, .y = 0};

    if (!offsetStack.empty()) previousOffset = offsetStack.back();

    offsetStack.push_back({previousOffset.x + offset.x, previousOffset.y + offset.y});

    renderer.GetContext().GetDirect2DDeviceContext()->SetTransform(GetCurrentTransform());
}

void ui::State::PopOffset()
{
    if (offsetStack.empty()) renderer.GetClient().GetContext().GetDebugLayer().AddWarning("PopOffset called on empty offset stack");
    else offsetStack.pop_back();

    renderer.GetContext().GetDirect2DDeviceContext()->SetTransform(GetCurrentTransform());
}

void ui::State::PushClip(RectangleF const rectangle)
{
    clipStackCounter += 1;

    renderer.GetContext().GetDirect2DDeviceContext()->PushAxisAlignedClip(rectangle.ToD2D1(), AntialiasMode);
}

void ui::State::PopClip()
{
    if (clipStackCounter == 0) renderer.GetClient().GetContext().GetDebugLayer().AddWarning("PopClip called on empty clip stack");
    else
    {
        clipStackCounter -= 1;

        renderer.GetContext().GetDirect2DDeviceContext()->PopAxisAlignedClip();
    }
}

void ui::State::PushOpacity(FLOAT const opacity)
{
    opacityStackCounter += 1;

    D2D1_LAYER_PARAMETERS1 options = D2D1::LayerParameters1();
    options.opacity                = opacity;
    options.maskAntialiasMode      = AntialiasMode;

    renderer.GetContext().GetDirect2DDeviceContext()->PushLayer(options, nullptr);
}

void ui::State::PopOpacity()
{
    if (opacityStackCounter == 0) renderer.GetClient().GetContext().GetDebugLayer().AddWarning("PopOpacity called on empty opacity stack");
    else
    {
        opacityStackCounter -= 1;

        renderer.GetContext().GetDirect2DDeviceContext()->PopLayer();
    }
}

void ui::State::Validate() const
{
    if (!offsetStack.empty()) renderer.GetClient().GetContext().GetDebugLayer().AddWarning("Offset stack is not empty");

    if (clipStackCounter > 0) renderer.GetClient().GetContext().GetDebugLayer().AddWarning("Clip stack is not empty");

    if (opacityStackCounter > 0) renderer.GetClient().GetContext().GetDebugLayer().AddWarning("Opacity stack is not empty");
}

D2D1_MATRIX_3X2_F ui::State::GetCurrentTransform() const
{
    auto const& [x, y] = offsetStack.empty() ? PointF{0, 0} : offsetStack.back();

    return D2D1::Matrix3x2F::Translation(x, y);
}
