#include "stdafx.h"

ui::Renderer::Renderer(NativeClient& client, INT const priority)
    : Object(client)
  , priority(priority)
  , brushSupport(*this)
  , textFormatSupport(*this)
  , textSupport(*this)
  , strokeSupport(*this)
{
    // todo: Create the UI renderer.
    // Contract: Renderer is the only UI renderer type and owns exactly one ui::Context.
}

INT ui::Renderer::GetPriority() const { return priority; }

ui::BrushSupport& ui::Renderer::GetBrushSupport() { return brushSupport; }

ui::TextFormatSupport& ui::Renderer::GetTextFormatSupport() { return textFormatSupport; }

ui::TextSupport& ui::Renderer::GetTextSupport() { return textSupport; }

ui::StrokeSupport& ui::Renderer::GetStrokeSupport() { return strokeSupport; }

void ui::Renderer::SubmitCommands(Command const* newCommands, UINT const newCommandCount)
{
    // todo: Copy the submitted command span into commands.
    // Contract: the caller retains no ownership after the call; only the latest submitted list is rendered.
    (void)newCommands;
    (void)newCommandCount;
}

void ui::Renderer::Render(UINT const frameIndex)
{
    // todo: Render the latest submitted command list into the final swap-chain back buffer for frameIndex.
    // Contract: acquire/release wrapped resources around the Direct2D draw pass, validate command kinds and handles
    // in NATIVE_DEBUG, skip invalid geometry as specified, and validate empty state stacks after rendering.
    (void)frameIndex;
}

void ui::Renderer::Free()
{
    // todo: Free this renderer through NativeClient ownership management.
    // Contract: validate in NATIVE_DEBUG that no Brush, TextFormat, or Text owned by this renderer is still active.
}
