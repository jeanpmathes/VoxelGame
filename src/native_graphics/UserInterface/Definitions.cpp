#include "stdafx.h"

D2D1_POINT_2F ui::PointF::ToD2D1() const
{
    return D2D1::Point2F(x, y);
}

D2D1_SIZE_F ui::SizeF::ToD2D1() const
{
    return D2D1::SizeF(width, height);
}

D2D1_RECT_F ui::RectangleF::ToD2D1() const
{
    FLOAT const left   = x;
    FLOAT const top    = y;
    FLOAT const right  = x + width;
    FLOAT const bottom = y + height;

    return D2D1::RectF(left, top, right, bottom);
}

D2D1_ROUNDED_RECT ui::RectangleF::ToD2D1(RadiusF const& radius) const
{
    return D2D1::RoundedRect(ToD2D1(), radius.x, radius.y);
}

D2D1_COLOR_F ui::ColorF::ToD2D1() const
{
    return D2D1::ColorF(r, g, b, a);
}
