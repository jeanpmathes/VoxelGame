//  <copyright file="native.cpp" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#include "stdafx.h"

static NativeErrorFunc onError;
static NativeErrorMessageFunc onErrorMessage;

NATIVE void NativeShowErrorBox(const LPCWSTR message, const LPCWSTR caption)
{
    // No try-catch because the catch might call this function again.
    Win32Application::ShowErrorMessage(message, caption);
}

NATIVE NativeClient* NativeConfigure(const Configuration config, const NativeErrorFunc errorCallback,
                                     const NativeErrorMessageFunc errorMessageCallback)
{
    onError = errorCallback;
    onErrorMessage = errorMessageCallback;

    TRY
    {
        return new NativeClient(config);
    }
    CATCH();
}

NATIVE void NativeFinalize(const NativeClient* client)
{
    TRY
    {
        REQUIRE(CALL_OUTSIDE_CYCLE(client));

        delete client;

#if defined(VG_DEBUG)
        IDXGIDebug1* pDebug = nullptr;
        if (SUCCEEDED(DXGIGetDebugInterface1(0, IID_PPV_ARGS(&pDebug))))
        {
            pDebug->ReportLiveObjects(DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_ALL);
            pDebug->Release();
        }
#endif
    }
    CATCH();
}

NATIVE void NativeRequestClose(const NativeClient* client)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(client));
        REQUIRE(Win32Application::IsRunning(client));

        PostMessage(Win32Application::GetHwnd(), WM_CLOSE, 0, 0);
    }
    CATCH();
}

NATIVE int NativeRun(NativeClient* client, const int nCmdShow)
{
    TRY
    {
        REQUIRE(CALL_OUTSIDE_CYCLE(client));

        return Win32Application::Run(client, GetModuleHandle(nullptr), nCmdShow);
    }
    CATCH();
}

NATIVE void NativePassAllocatorStatistics(const NativeClient* client, const NativeWStringFunc receiver)
{
    TRY
    {
        REQUIRE(CALL_ON_MAIN_THREAD(client));

        LPWSTR statistics;
        client->GetAllocator()->BuildStatsString(&statistics, TRUE);

        receiver(statistics);

        client->GetAllocator()->FreeStatsString(statistics);
    }
    CATCH();
}

NATIVE void NativePassDRED(const NativeClient* client, const NativeWStringFunc receiver)
{
    TRY
    {
        REQUIRE(CALL_IN_RENDER(client));

        const std::wstring dred = client->GetDRED();
        receiver(const_cast<LPWSTR>(dred.c_str()));
    }
    CATCH();
}

NATIVE void NativeToggleFullscreen(const NativeClient* client)
{
    TRY
    {
        REQUIRE(CALL_ON_MAIN_THREAD(client));

        client->ToggleFullscreen();
    }
    CATCH();
}

NATIVE void NativeGetMousePosition(const NativeClient* client, PLONG x, PLONG y)
{
    TRY
    {
        const POINT position = client->GetMousePosition();

        *x = position.x;
        *y = position.y;
    }
    CATCH();
}

NATIVE void NativeSetMousePosition(const NativeClient* client, const LONG x, const LONG y)
{
    TRY
    {
        const POINT position = {x, y};

        client->SetMousePosition(position);
    }
    CATCH();
}

NATIVE void NativeSetCursor(const NativeClient* client, MouseCursor cursor)
{
    TRY
    {
        client->SetMouseCursor(cursor);
    }
    CATCH();
}

NATIVE void NativeInitializeRaytracing(NativeClient* client,
                                       ShaderFileDescription* shaderFiles,
                                       LPWSTR* symbols,
                                       MaterialDescription* materials,
                                       Texture** textures,
                                       const SpacePipelineDescription description)
{
    TRY
    {
        REQUIRE(CALL_OUTSIDE_CYCLE(client));

        client->InitRaytracingPipeline({
            shaderFiles,
            symbols,
            materials,
            textures,
            description
        });
    }
    CATCH();
}

NATIVE Camera* NativeGetCamera(const NativeClient* client)
{
    TRY
    {
        return client->GetSpace()->GetCamera();
    }
    CATCH();
}

NATIVE Light* NativeGetLight(const NativeClient* client)
{
    TRY
    {
        return client->GetSpace()->GetLight();
    }
    CATCH();
}

NATIVE void NativeSetLightDirection(Light* light, const DirectX::XMFLOAT3 direction)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(&light->GetClient()));

        light->SetDirection(direction);
    }
    CATCH();
}

NATIVE void NativeUpdateBasicCameraData(Camera* camera, const BasicCameraData data)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(&camera->GetClient()));

        camera->SetPosition(data.position);
        camera->SetOrientation(data.front, data.up);
    }
    CATCH();
}

NATIVE void NativeUpdateAdvancedCameraData(Camera* camera, const AdvancedCameraData data)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(&camera->GetClient()));

        camera->SetFov(data.fov);
        camera->SetPlanes(data.nearDistance, data.farDistance);
    }
    CATCH();
}

NATIVE void NativeUpdateSpatialObjectData(SpatialObject* object, const SpatialObjectData data)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(&object->GetClient()));

        object->SetPosition(data.position);
        object->SetRotation(data.rotation);
    }
    CATCH();
}

NATIVE MeshObject* NativeCreateMeshObject(const NativeClient* client, const UINT materialIndex)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(client));

        return &client->GetSpace()->CreateMeshObject(materialIndex);
    }
    CATCH();
}

NATIVE void NativeFreeMeshObject(const MeshObject* object)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(&object->GetClient()));

        object->Free();
    }
    CATCH();
}

NATIVE void NativeSetMeshObjectEnabledState(MeshObject* object, const bool enabled)
{
    TRY
    {
        REQUIRE(CALL_INSIDE_CYCLE(&object->GetClient()));

        object->SetEnabledState(enabled);
    }
    CATCH();
}

NATIVE void NativeSetMeshObjectMesh(MeshObject* object, const SpatialVertex* vertexData, const UINT vertexCount)
{
    TRY
    {
        REQUIRE(CALL_IN_UPDATE(&object->GetClient()));

        object->SetNewMesh(vertexData, vertexCount);
    }
    CATCH();
}

NATIVE RasterPipeline* NativeCreateRasterPipeline(NativeClient* client,
                                                  const PipelineDescription description,
                                                  const NativeErrorFunc callback)
{
    TRY
    {
        REQUIRE(CALL_OUTSIDE_CYCLE(client));

        std::unique_ptr<RasterPipeline> pipeline = RasterPipeline::Create(*client, description, callback);
        RasterPipeline* ptr = pipeline.get();

        if (ptr != nullptr) client->AddRasterPipeline(std::move(pipeline));

        return ptr;
    }
    CATCH();
}

NATIVE ShaderBuffer* NativeGetShaderBuffer(const RasterPipeline* pipeline)
{
    TRY
    {
        if (pipeline == nullptr) return nullptr;
        return pipeline->GetShaderBuffer();
    }
    CATCH();
}

NATIVE void NativeDesignatePostProcessingPipeline(NativeClient* client, RasterPipeline* pipeline)
{
    TRY
    {
        REQUIRE(CALL_OUTSIDE_CYCLE(client));

        client->SetPostProcessingPipeline(pipeline);
    }
    CATCH();
}

NATIVE void NativeSetShaderBufferData(const ShaderBuffer* buffer, const void* data)
{
    TRY
    {
        REQUIRE(CALL_ON_MAIN_THREAD(&buffer->GetClient()));

        buffer->SetData(data);
    }
    CATCH();
}

NATIVE void NativeAddDraw2DPipeline(NativeClient* client, RasterPipeline* pipeline, const draw2d::Callback callback)
{
    TRY
    {
        REQUIRE(CALL_OUTSIDE_CYCLE(client));

        client->AddDraw2DPipeline(pipeline, callback);
    }
    CATCH();
}

NATIVE Texture* NativeLoadTexture(const NativeClient* client, std::byte** data, const TextureDescription description)
{
    TRY
    {
        REQUIRE(CALL_ON_MAIN_THREAD(client));

        return client->LoadTexture(data, description);
    }
    CATCH();
}

NATIVE void NativeFreeTexture(const Texture* texture)
{
    TRY
    {
        REQUIRE(CALL_ON_MAIN_THREAD(&texture->GetClient()));

        texture->Free();
    }
    CATCH();
}
