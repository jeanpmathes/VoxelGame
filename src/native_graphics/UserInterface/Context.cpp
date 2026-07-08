#include "stdafx.h"

ui::Context::Context(::Context& context)
    : context(context)
{
    dpi = static_cast<FLOAT>(GetDpiForWindow(Win32Application::GetWindowHandle()));

    D2D1_FACTORY_OPTIONS factoryOptions;
#ifdef NATIVE_DEBUG
    factoryOptions.debugLevel = D2D1_DEBUG_LEVEL_INFORMATION;
#else
    factoryOptions.debugLevel = D2D1_DEBUG_LEVEL_NONE;
#endif

    TryDo(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, __uuidof(ID2D1Factory3), &factoryOptions, &direct2DFactory));

    ComPtr<IDXGIDevice> dxgiDevice;
    TryDo(context.GetD3D11On12Device().As(&dxgiDevice));
    {
        // Direct2D with Direct11On12 creates warnings, not related to any bugs in this code.
        // So we ignore them with filters until this issue gets fixed at some point.
        // Grrrrrrr Microsoft!

        auto filter = context.GetDebugLayer().PushFilter(D3D12_MESSAGE_ID_CREATERESOURCE_STATE_IGNORED);
        TryDo(direct2DFactory->CreateDevice(dxgiDevice.Get(), &direct2DDevice));
    }
    TryDo(direct2DDevice->CreateDeviceContext(D2D1_DEVICE_CONTEXT_OPTIONS_NONE, &direct2DDeviceContext));

    TryDo(DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory8), &directWriteFactory));
}

void ui::Context::ReleaseRenderTargets()
{
    renderTargets = {};

    direct2DDeviceContext->SetTarget(nullptr);
    direct2DDevice->ClearResources(0);

    context.GetD3D11DeviceContext()->ClearState();
    context.GetD3D11DeviceContext()->Flush();
}

void ui::Context::CreateRenderTargets()
{
    D2D1_BITMAP_PROPERTIES1 const bitmapProperties = D2D1::BitmapProperties1(
                                                                             D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS_CANNOT_DRAW,
                                                                             D2D1::PixelFormat(DXGI_FORMAT_UNKNOWN, D2D1_ALPHA_MODE_PREMULTIPLIED),
                                                                             dpi,
                                                                             dpi);

    for (UINT frame = 0; frame < FRAME_COUNT; frame++)
    {
        auto& [wrappedBackBuffer, surface, bitmap] = renderTargets[frame];

        D3D11_RESOURCE_FLAGS flags = {.BindFlags = D3D11_BIND_RENDER_TARGET};
        TryDo(
              context.GetD3D11On12Device()->CreateWrappedResource(
                                                                  context.GetFinalRenderTarget(frame).Get(),
                                                                  &flags,
                                                                  D3D12_RESOURCE_STATE_RENDER_TARGET,
                                                                  D3D12_RESOURCE_STATE_PRESENT,
                                                                  IID_PPV_ARGS(&wrappedBackBuffer)));

        NAME_DIRECT_OBJECT(wrappedBackBuffer);

        TryDo(wrappedBackBuffer.As(&surface));
        TryDo(direct2DDeviceContext->CreateBitmapFromDxgiSurface(surface.Get(), &bitmapProperties, &bitmap));

        NAME_DIRECT_OBJECT(surface);
    }
}

void ui::Context::BeginFrame(UINT const frameIndex) const
{
    std::vector<ID3D11Resource*> const wrappedResources = GetWrappedResources(frameIndex);
    context.GetD3D11On12Device()->AcquireWrappedResources(wrappedResources.data(), static_cast<UINT>(wrappedResources.size()));

    direct2DDeviceContext->SetTarget(renderTargets[frameIndex].bitmap.Get());
}

void ui::Context::EndFrame(UINT const frameIndex) const
{
    std::vector<ID3D11Resource*> const wrappedResources = GetWrappedResources(frameIndex);
    context.GetD3D11On12Device()->ReleaseWrappedResources(wrappedResources.data(), static_cast<UINT>(wrappedResources.size()));

    context.GetD3D11DeviceContext()->Flush();
}

IDWriteFactory8* ui::Context::GetDirectWriteFactory() const { return directWriteFactory.Get(); }

ID2D1Factory3* ui::Context::GetDirect2DFactory() const
{
    return direct2DFactory.Get();
}

ID2D1DeviceContext* ui::Context::GetDirect2DDeviceContext() const
{
    return direct2DDeviceContext.Get();
}

std::vector<ID3D11Resource*> ui::Context::GetWrappedResources(UINT const frameIndex) const
{
    std::vector<ID3D11Resource*> wrappedResources;

    wrappedResources.push_back(renderTargets[frameIndex].wrappedBackBuffer.Get());

    return wrappedResources;
}
