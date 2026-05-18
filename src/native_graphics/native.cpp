//  <copyright file="native.cpp" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#include "stdafx.h"
#include "UserInterface/Objects/Brush.hpp"
#include "UserInterface/Objects/Renderer.hpp"
#include "UserInterface/Objects/Text.hpp"
#include "UserInterface/Objects/TextFormat.hpp"

namespace
{
    NativeErrorFunction onError;
}

NATIVE void NativeShowErrorBox(LPCWSTR const message, LPCWSTR const caption)
{
    // No try-catch because the catch might call this function again.
    Win32Application::ShowErrorMessage(message, caption);
}

NATIVE NativeClient* NativeConfigure(Configuration const config, NativeErrorFunction const errorCallback)
{
    onError = errorCallback;

    TRY { return new NativeClient(config); } CATCH();
}

NATIVE void NativeFinalize(NativeClient const* client)
{
    TRY
    {
        delete client;

#if defined(NATIVE_DEBUG)
        IDXGIDebug1* debug = nullptr;
        if (SUCCEEDED(DXGIGetDebugInterface1(0, IID_PPV_ARGS(&debug))))
        {
            HRESULT const result = debug->ReportLiveObjects(DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_ALL);
            debug->Release();

            (void)result;
        }
#endif
    } CATCH();
}

NATIVE void NativeRequestClose(NativeClient const* client)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        PostMessage(Win32Application::GetWindowHandle(), WM_CLOSE, 0, 0);
    } CATCH();
}

NATIVE int NativeRun(NativeClient* client)
{
    TRY
    {
        Require(CALL_OUTSIDE_CYCLE(client));

        return Win32Application::Run(client, GetModuleHandle(nullptr), 1);
    } CATCH();
}

NATIVE void NativeSetTimeScale(NativeClient* client, double const timeScale)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        client->SetTimeScale(timeScale);
    } CATCH();
}

NATIVE void NativePassAllocatorStatistics(NativeClient const* client, NativeWStringFunction const receiver)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        LPWSTR statistics;
        client->GetContext().GetAllocator()->BuildStatsString(&statistics, TRUE);

        receiver(statistics);

        client->GetContext().GetAllocator()->FreeStatsString(statistics);
    } CATCH();
}

NATIVE void NativePassDRED(NativeClient const* client, NativeWStringFunction const receiver)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        std::wstring const dred = client->GetDRED();
        receiver(dred.c_str());
    } CATCH();
}

NATIVE void NativeTakeScreenshot(NativeClient* client, ScreenshotFunc const func)
{
    TRY
    {
        Require(CALL_IN_LOGIC(client));

        client->TakeScreenshot(func);
    } CATCH();
}

NATIVE void NativeToggleFullscreen(NativeClient const* client)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        client->ToggleFullscreen();
    } CATCH();
}

NATIVE void NativeGetMousePosition(NativeClient const* client, PLONG const x, PLONG const y)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        POINT const position = client->GetMousePosition();

        *x = position.x;
        *y = position.y;
    } CATCH();
}

NATIVE void NativeSetMousePosition(NativeClient* client, LONG const x, LONG const y)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        POINT const position = {x, y};

        client->SetMousePosition(position);
    } CATCH();
}

NATIVE void NativeSetCursorType(NativeClient* client, MouseCursor cursor)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(client));

        client->SetMouseCursor(cursor);
    } CATCH();
}

NATIVE void NativeSetCursorLock(NativeClient* client, bool const lock)
{
    TRY { client->SetMouseLock(lock); } CATCH();
}

NATIVE ShaderBuffer* NativeInitializeRaytracing(NativeClient* client, SpacePipelineDescription const description)
{
    TRY
    {
        Require(CALL_OUTSIDE_CYCLE(client));

        client->InitRaytracingPipeline(description);

        if (client->GetSpace() == nullptr) return nullptr;

        return client->GetSpace()->GetCustomDataBuffer();
    } CATCH();
}

NATIVE Camera* NativeGetCamera(NativeClient const* client)
{
    TRY { return client->GetSpace()->GetCamera(); } CATCH();
}

NATIVE Light* NativeGetLight(NativeClient const* client)
{
    TRY { return client->GetSpace()->GetLight(); } CATCH();
}

NATIVE void NativeSetSpaceIsRendered(NativeClient const* client, bool const isRendered)
{
    TRY
    {
        Require(CALL_IN_LOGIC(client));

        client->GetSpace()->SetIsRendered(isRendered);
    } CATCH();
}

NATIVE void NativeSetLightConfiguration(Light* light, DirectX::XMFLOAT3 const direction, DirectX::XMFLOAT3 const color, float const intensity)
{
    TRY
    {
        Require(CALL_IN_LOGIC(&light->GetClient()));

        light->SetDirection(direction);
        light->SetColor(color);
        light->SetIntensity(intensity);
    } CATCH();
}

NATIVE void NativeUpdateBasicCameraData(Camera* camera, BasicCameraData const data)
{
    TRY
    {
        Require(CALL_IN_LOGIC_OR_EVENT(&camera->GetClient()));

        camera->SetPosition(data.position);
        camera->SetOrientation(data.front, data.up);
    } CATCH();
}

NATIVE void NativeUpdateAdvancedCameraData(Camera* camera, AdvancedCameraData const data)
{
    TRY
    {
        Require(CALL_IN_LOGIC_OR_EVENT(&camera->GetClient()));

        camera->SetFov(data.fov);
        camera->SetPlanes(data.nearDistance, data.farDistance);
    } CATCH();
}

NATIVE void NativeUpdateSpatialData(Spatial* object, SpatialData const data)
{
    TRY
    {
        Require(CALL_IN_LOGIC(&object->GetClient()));

        object->SetPosition(data.position);
        object->SetRotation(data.rotation);
    } CATCH();
}

NATIVE Mesh* NativeCreateMesh(NativeClient const* client, UINT const materialIndex)
{
    TRY
    {
        Require(CALL_IN_LOGIC(client));

        return &client->GetSpace()->CreateMesh(materialIndex);
    } CATCH();
}

NATIVE void NativeSetMeshVertices(Mesh* object, SpatialVertex const* vertexData, UINT const vertexCount)
{
    TRY
    {
        Require(CALL_IN_LOGIC(&object->GetClient()));

        object->SetNewVertices(vertexData, vertexCount);
    } CATCH();
}

NATIVE void NativeSetMeshBounds(Mesh* object, SpatialBounds const* boundsData, UINT const boundsCount)
{
    TRY
    {
        Require(CALL_IN_LOGIC(&object->GetClient()));

        object->SetNewBounds(boundsData, boundsCount);
    } CATCH();
}

NATIVE Effect* NativeCreateEffect(NativeClient const* client, RasterPipeline* pipeline)
{
    TRY
    {
        Require(CALL_IN_LOGIC(client));

        return &client->GetSpace()->CreateEffect(pipeline);
    } CATCH();
}

NATIVE void NativeSetEffectVertices(Effect* object, EffectVertex const* vertexData, UINT const vertexCount)
{
    TRY
    {
        Require(CALL_IN_LOGIC(&object->GetClient()));

        object->SetNewVertices(vertexData, vertexCount);
    } CATCH();
}

NATIVE void NativeReturnDrawable(Drawable* object)
{
    TRY
    {
        Require(CALL_IN_LOGIC(&object->GetClient()));

        object->Return();
    } CATCH();
}

NATIVE void NativeSetDrawableEnabledState(Drawable* object, bool const enabled)
{
    TRY
    {
        Require(CALL_INSIDE_CYCLE(&object->GetClient()));

        object->SetEnabledState(enabled);
    } CATCH();
}

NATIVE RasterPipeline* NativeCreateRasterPipeline(NativeClient* client, RasterPipelineDescription const description, NativeErrorFunction const callback)
{
    TRY
    {
        Require(CALL_OUTSIDE_CYCLE(client));

        std::unique_ptr<RasterPipeline> pipeline = RasterPipeline::Create(*client, description, callback);
        RasterPipeline*                 ptr      = pipeline.get();

        if (ptr != nullptr) client->AddRasterPipeline(std::move(pipeline));

        return ptr;
    } CATCH();
}

NATIVE ShaderBuffer* NativeGetRasterPipelineShaderBuffer(RasterPipeline const* pipeline)
{
    TRY { return pipeline->GetShaderBuffer(); } CATCH();
}

NATIVE void NativeDesignatePostProcessingPipeline(NativeClient* client, RasterPipeline* pipeline)
{
    TRY
    {
        Require(CALL_OUTSIDE_CYCLE(client));

        client->SetPostProcessingPipeline(pipeline);
    } CATCH();
}

NATIVE void NativeSetShaderBufferData(ShaderBuffer const* buffer, std::byte const* data)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(&buffer->GetClient()));

        buffer->SetData(data);
    } CATCH();
}

NATIVE UINT NativeAddDraw2DPipeline(NativeClient* client, RasterPipeline* pipeline, INT const priority, draw2d::Callback const callback)
{
    TRY
    {
        Require(CALL_OUTSIDE_CYCLE(client));

        return client->AddDraw2DPipeline(pipeline, priority, callback);
    } CATCH();
}

NATIVE void NativeRemoveDraw2DPipeline(NativeClient* client, UINT const id)
{
    TRY
    {
        Require(CALL_OUTSIDE_CYCLE(client));

        client->RemoveDraw2DPipeline(id);
    } CATCH();
}

NATIVE Texture* NativeLoadTexture(NativeClient const* client, std::byte** data, TextureDescription const description)
{
    TRY
    {
        Require(CALL_OUTSIDE_CYCLE(client) || CALL_IN_RENDER(client));

        return client->LoadTexture(data, description);
    } CATCH();
}

NATIVE void NativeFreeTexture(Texture const* texture)
{
    TRY
    {
        Require(CALL_ON_MAIN_THREAD(&texture->GetClient()));

        texture->Free();
    } CATCH();
}

NATIVE ui::Renderer* NativeCreateUserInterface(NativeClient* client, INT priority)
{
    TRY
    {
        // todo: Create a ui::Renderer through NativeClient::CreateUserInterface.
        // Contract: creating a UI renderer is allowed outside render and logic cycles; lower priority renders first,
        // higher priority appears on top, and equal priority preserves insertion order.
        (void)client;
        (void)priority;
        throw NativeException("TODO: create UI renderer.");
    } CATCH();
}

NATIVE void NativeFreeUserInterface(ui::Renderer* renderer)
{
    TRY
    {
        // todo: Call ui::Renderer::Free, which delegates to NativeClient::FreeUserInterface.
        // Contract: validate that no active UI resources still belong to the renderer; the pointer is invalid after this call.
        (void)renderer;
    } CATCH();
}

NATIVE void NativeSubmitUserInterfaceCommands(ui::Renderer* renderer, ui::Command const* commands, UINT commandCount)
{
    TRY
    {
        // todo: Submit commands to the UI renderer.
        // Contract: allowed outside UI render execution and from the main thread; renderer copies the command span.
        (void)renderer;
        (void)commands;
        (void)commandCount;
    } CATCH();
}

NATIVE ui::Brush* NativeCreateUserInterfaceSolidColorBrush(ui::Renderer* renderer, ui::ColorF color)
{
    TRY
    {
        // todo: Create a solid-color UI brush through renderer->GetContext().GetBrushSupport().
        // Contract: creating/freeing UI resources is allowed outside UI render execution and from the main thread.
        (void)renderer;
        (void)color;
        throw NativeException("TODO: create UI solid-color brush.");
    } CATCH();
}

NATIVE void NativeFreeUserInterfaceBrush(ui::Brush* brush)
{
    TRY
    {
        // todo: Return the brush through ui::Brush::Return.
        // Contract: the disposed C# wrapper must never be used again even if UI storage is reused.
        (void)brush;
    } CATCH();
}

NATIVE ui::TextFormat* NativeCreateUserInterfaceTextFormat(ui::Renderer* renderer, ui::TextFormatDescription description)
{
    TRY
    {
        // todo: Create a UI text format through renderer->GetContext().GetTextFormatSupport().
        // Contract: the UI text-format object must not retain the local-only fontFamily pointer from description.
        (void)renderer;
        (void)description;
        throw NativeException("TODO: create UI text format.");
    } CATCH();
}

NATIVE void NativeFreeUserInterfaceTextFormat(ui::TextFormat* format)
{
    TRY
    {
        // todo: Return the text format through ui::TextFormat::Return.
        // Contract: reusable UI storage does not make a disposed C# wrapper usable again.
        (void)format;
    } CATCH();
}

NATIVE ui::Text* NativeCreateUserInterfaceText(ui::Renderer* renderer, LPCWSTR text, ui::TextFormat* format)
{
    TRY
    {
        // todo: Create a UI text layout through renderer->GetContext().GetTextSupport().
        // Contract: text objects are not pooled and use the supplied text format.
        (void)renderer;
        (void)text;
        (void)format;
        throw NativeException("TODO: create UI text.");
    } CATCH();
}

NATIVE void NativeFreeUserInterfaceText(ui::Text* text)
{
    TRY
    {
        // todo: Return the text through ui::Text::Return.
        // Contract: TextSupport removes and destroys text layouts instead of pooling them.
        (void)text;
    } CATCH();
}

NATIVE ui::SizeF NativeMeasureUserInterfaceText(ui::Text* text, ui::SizeF availableSize)
{
    TRY
    {
        // todo: Measure text through ui::Text::Measure.
        // Contract: measuring is allowed from the C# GUI layout path and must not require an active Direct2D draw pass.
        (void)text;
        (void)availableSize;
        throw NativeException("TODO: measure UI text.");
    } CATCH();
}
