#include "stdafx.h"

ui::StrokeSupport::StrokeSupport(Renderer& renderer)
    : renderer(renderer)
{
    auto createStrokeStyle = [&](D2D1_CAP_STYLE const capStyle, D2D1_DASH_STYLE const dashStyle, ID2D1StrokeStyle** style)
    {
        D2D1_STROKE_STYLE_PROPERTIES properties = D2D1::StrokeStyleProperties();
        properties.startCap                     = capStyle;
        properties.endCap                       = capStyle;
        properties.dashCap                      = capStyle;
        properties.dashStyle                    = dashStyle;

        TryDo(renderer.GetContext().GetDirect2DFactory()->CreateStrokeStyle(properties, nullptr, 0, style));
    };

    createStrokeStyle(D2D1_CAP_STYLE_FLAT, D2D1_DASH_STYLE_SOLID, &solid);
    createStrokeStyle(D2D1_CAP_STYLE_FLAT, D2D1_DASH_STYLE_DASH, &dashes);
    createStrokeStyle(D2D1_CAP_STYLE_FLAT, D2D1_DASH_STYLE_DOT, &squared);
    createStrokeStyle(D2D1_CAP_STYLE_ROUND, D2D1_DASH_STYLE_DOT, &dotted);
}

ID2D1StrokeStyle* ui::StrokeSupport::UseRawStrokeStyle(StrokeStyle const style) const
{
    switch (style)
    {
    case StrokeStyle::SOLID:
        return solid.Get();
    case StrokeStyle::DASHES:
        return dashes.Get();
    case StrokeStyle::SQUARED:
        return squared.Get();
    case StrokeStyle::DOTTED:
        return dotted.Get();

    default:
        throw NativeException("Stroke style not implemented.");
    }
}

void ui::StrokeSupport::ValidateAllWrappedResourcesAreReturned() const
{
    // This support class does not create wrappers that have to be returned.
    // As such, no special validation is necessary.

    (void)this;
}
