#include "stdafx.h"

ui::Renderer::Renderer(NativeClient& client)
    : Object(client)
  , state(*this)
  , brushSupport(*this)
  , textFormatSupport(*this)
  , textSupport(*this)
  , strokeSupport(*this)
{
}

ui::Context& ui::Renderer::GetContext() const
{
    return GetClient().GetContext().GetUserInterfaceContext();
}

ui::BrushSupport& ui::Renderer::GetBrushSupport() { return brushSupport; }

ui::TextFormatSupport& ui::Renderer::GetTextFormatSupport() { return textFormatSupport; }

ui::TextSupport& ui::Renderer::GetTextSupport() { return textSupport; }

ui::StrokeSupport& ui::Renderer::GetStrokeSupport() { return strokeSupport; }

void ui::Renderer::SubmitCommands(Command const* newCommands, UINT const newCommandCount)
{
    Command const* begin = newCommands;
    Command const* end   = newCommands + newCommandCount;

    commands.clear();
    commands.insert(commands.end(), begin, end);
}

void ui::Renderer::Render()
{
    GetContext().GetDirect2DDeviceContext()->BeginDraw();

    for (Command const& command : commands)
        switch (command.kind)
        {
        case CommandKind::PUSH_OFFSET:
            PushOffset(command.pushOffset);
            break;
        case CommandKind::POP_OFFSET:
            PopOffset();
            break;
        case CommandKind::PUSH_CLIP:
            PushClip(command.pushClip);
            break;
        case CommandKind::POP_CLIP:
            PopClip();
            break;
        case CommandKind::PUSH_OPACITY:
            PushOpacity(command.pushOpacity);
            break;
        case CommandKind::POP_OPACITY:
            PopOpacity();
            break;
        case CommandKind::DRAW_RECTANGLE_LINED_COLOR:
            DrawRectangleLinedColor(command.drawRectangleLinedColor);
            break;
        case CommandKind::DRAW_RECTANGLE_LINED_BRUSH:
            DrawRectangleLinedBrush(command.drawRectangleLinedBrush);
            break;
        case CommandKind::DRAW_RECTANGLE_LINED_ROUNDED_COLOR:
            DrawRectangleLinedRoundedColor(command.drawRectangleLinedRoundedColor);
            break;
        case CommandKind::DRAW_RECTANGLE_LINED_ROUNDED_BRUSH:
            DrawRectangleLinedRoundedBrush(command.drawRectangleLinedRoundedBrush);
            break;
        case CommandKind::DRAW_RECTANGLE_FILLED_COLOR:
            DrawRectangleFilledColor(command.drawRectangleFilledColor);
            break;
        case CommandKind::DRAW_RECTANGLE_FILLED_BRUSH:
            DrawRectangleFilledBrush(command.drawRectangleFilledBrush);
            break;
        case CommandKind::DRAW_RECTANGLE_FILLED_ROUNDED_COLOR:
            DrawRectangleFilledRoundedColor(command.drawRectangleFilledRoundedColor);
            break;
        case CommandKind::DRAW_RECTANGLE_FILLED_ROUNDED_BRUSH:
            DrawRectangleFilledRoundedBrush(command.drawRectangleFilledRoundedBrush);
            break;
        case CommandKind::DRAW_TEXT_COLOR:
            DrawTextColor(command.drawTextColor);
            break;
        case CommandKind::DRAW_TEXT_BRUSH:
            DrawTextBrush(command.drawTextBrush);
            break;
        default:
            throw NativeException("Command not implemented.");
        }

#ifdef NATIVE_DEBUG
    state.Validate();
#endif

    {
        // Seems to be another bug of Direct2D.
        // Grrrrrrr Microsoft!

        auto filter = GetClient().GetContext().GetDebugLayer().PushFilter(D3D12_MESSAGE_ID_CREATERESOURCE_STATE_IGNORED);
        TryDo(GetContext().GetDirect2DDeviceContext()->EndDraw());
    }
}

void ui::Renderer::Free()
{
#ifdef NATIVE_DEBUG
    brushSupport.ValidateAllWrappedResourcesAreReturned();
    textFormatSupport.ValidateAllWrappedResourcesAreReturned();
    textSupport.ValidateAllWrappedResourcesAreReturned();
    strokeSupport.ValidateAllWrappedResourcesAreReturned();
#endif

    GetClient().FreeUserInterface(this);
}

void ui::Renderer::PushOffset(PushOffsetCommand const& command)
{
    state.PushOffset(command.offset);
}

void ui::Renderer::PopOffset()
{
    state.PopOffset();
}

void ui::Renderer::PushClip(PushClipCommand const& command)
{
    state.PushClip(command.clip);
}

void ui::Renderer::PopClip()
{
    state.PopClip();
}

void ui::Renderer::PushOpacity(PushOpacityCommand const& command)
{
    state.PushOpacity(command.opacity);
}

void ui::Renderer::PopOpacity()
{
    state.PopOpacity();
}

void ui::Renderer::DrawRectangleLinedColor(DrawRectangleLinedColorCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->DrawRectangle(
                                                           command.rectangle.ToD2D1(),
                                                           brushSupport.UseRawSolidColorBrush(command.color),
                                                           command.strokeWidth,
                                                           strokeSupport.UseRawStrokeStyle(command.strokeStyle)
                                                          );
}

void ui::Renderer::DrawRectangleLinedBrush(DrawRectangleLinedBrushCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->DrawRectangle(
                                                           command.rectangle.ToD2D1(),
                                                           command.brush->GetWrapped(),
                                                           command.strokeWidth,
                                                           strokeSupport.UseRawStrokeStyle(command.strokeStyle)
                                                          );
}

void ui::Renderer::DrawRectangleLinedRoundedColor(DrawRectangleLinedRoundedColorCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->DrawRoundedRectangle(
                                                                  command.rectangle.ToD2D1(command.radius),
                                                                  brushSupport.UseRawSolidColorBrush(command.color),
                                                                  command.strokeWidth,
                                                                  strokeSupport.UseRawStrokeStyle(command.strokeStyle)
                                                                 );
}

void ui::Renderer::DrawRectangleLinedRoundedBrush(DrawRectangleLinedRoundedBrushCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->DrawRoundedRectangle(
                                                                  command.rectangle.ToD2D1(command.radius),
                                                                  command.brush->GetWrapped(),
                                                                  command.strokeWidth,
                                                                  strokeSupport.UseRawStrokeStyle(command.strokeStyle)
                                                                 );
}

void ui::Renderer::DrawRectangleFilledColor(DrawRectangleFilledColorCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->FillRectangle(
                                                           command.rectangle.ToD2D1(),
                                                           brushSupport.UseRawSolidColorBrush(command.color)
                                                          );
}

// ReSharper disable once CppMemberFunctionMayBeConst
void ui::Renderer::DrawRectangleFilledBrush(DrawRectangleFilledBrushCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->FillRectangle(
                                                           command.rectangle.ToD2D1(),
                                                           command.brush->GetWrapped()
                                                          );
}

void ui::Renderer::DrawRectangleFilledRoundedColor(DrawRectangleFilledRoundedColorCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->FillRoundedRectangle(
                                                                  command.rectangle.ToD2D1(command.radius),
                                                                  brushSupport.UseRawSolidColorBrush(command.color)
                                                                 );
}

// ReSharper disable once CppMemberFunctionMayBeConst
void ui::Renderer::DrawRectangleFilledRoundedBrush(DrawRectangleFilledRoundedBrushCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->FillRoundedRectangle(
                                                                  command.rectangle.ToD2D1(command.radius),
                                                                  command.brush->GetWrapped()
                                                                 );
}

namespace
{
    constexpr D2D1_DRAW_TEXT_OPTIONS TextDrawOptions = D2D1_DRAW_TEXT_OPTIONS_CLIP | D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;
}

void ui::Renderer::DrawTextColor(DrawTextColorCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->DrawTextLayout(
                                                            command.position.ToD2D1(),
                                                            command.text->GetWrapped(),
                                                            brushSupport.UseRawSolidColorBrush(command.color),
                                                            TextDrawOptions
                                                           );
}

// ReSharper disable once CppMemberFunctionMayBeConst
void ui::Renderer::DrawTextBrush(DrawTextBrushCommand const& command)
{
    GetContext().GetDirect2DDeviceContext()->DrawTextLayout(
                                                            command.position.ToD2D1(),
                                                            command.text->GetWrapped(),
                                                            command.brush->GetWrapped(),
                                                            TextDrawOptions
                                                           );
}
