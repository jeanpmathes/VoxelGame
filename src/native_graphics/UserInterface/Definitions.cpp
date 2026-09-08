#include "stdafx.h"

D2D1_POINT_2F ui::Point::ToD2D1() const
{
    Require(std::isfinite(x));
    Require(std::isfinite(y));

    return D2D1::Point2F(x, y);
}

D2D1_SIZE_F ui::Size::ToD2D1() const
{
    Require(std::isfinite(width));
    Require(width >= 0.0f);
    Require(std::isfinite(height));
    Require(height >= 0.0f);

    return D2D1::SizeF(width, height);
}

D2D1_RECT_F ui::Rectangle::ToD2D1() const
{
    Require(std::isfinite(x));
    Require(std::isfinite(y));
    Require(std::isfinite(width));
    Require(width >= 0.0f);
    Require(std::isfinite(height));
    Require(height >= 0.0f);

    FLOAT const left   = x;
    FLOAT const top    = y;
    FLOAT const right  = x + width;
    FLOAT const bottom = y + height;

    return D2D1::RectF(left, top, right, bottom);
}

D2D1_ROUNDED_RECT ui::Rectangle::ToD2D1(Radius const& radius) const
{
    Require(std::isfinite(radius.x));
    Require(radius.x >= 0.0f);
    Require(std::isfinite(radius.y));
    Require(radius.y >= 0.0f);

    return D2D1::RoundedRect(ToD2D1(), radius.x, radius.y);
}

D2D1_COLOR_F ui::Color::ToD2D1() const
{
    Require(std::isfinite(r));
    Require(std::isfinite(g));
    Require(std::isfinite(b));
    Require(std::isfinite(a));

    return D2D1::ColorF(r, g, b, a);
}
