//  <copyright file="native.cpp" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#include "stdafx.h"

static NativeErrorFunc onError;
static NativeErrorMessageFunc onErrorMessage;

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
        delete client;

#ifdef _DEBUG
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
        REQUIRE(Win32Application::IsRunning(client));
        PostMessage(Win32Application::GetHwnd(), WM_CLOSE, 0, 0);
    }
    CATCH();
}

NATIVE int NativeRun(NativeClient* client, const int nCmdShow)
{
    TRY
    {
        return Win32Application::Run(client, GetModuleHandle(nullptr), nCmdShow);
    }
    CATCH();
}

NATIVE void NativeSetResolution(NativeClient* client, const UINT width, const UINT height)
{
    TRY
    {
        client->SetResolution(width, height);
    }
    CATCH();
}

NATIVE void NativeToggleFullscreen(const NativeClient* client)
{
    TRY
    {
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

NATIVE Camera* NativeGetCamera(NativeClient* client)
{
    TRY
    {
        return client->GetSpace()->GetCamera();
    }
    CATCH();
}

NATIVE Light* NativeGetLight(NativeClient* client)
{
    TRY
    {
        return client->GetSpace()->GetLight();
    }
    CATCH();
}

NATIVE void NativeUpdateCameraData(Camera* camera, const CameraData data)
{
    TRY
    {
        camera->SetPosition(data.position);
    }
    CATCH();
}

NATIVE void NativeUpdateSpatialObjectData(SpatialObject* object, const SpatialObjectData data)
{
    TRY
    {
        object->SetPosition(data.position);
        object->SetRotation(data.rotation);
    }
    CATCH();
}

NATIVE SequencedMeshObject* NativeCreateSequencedMeshObject(NativeClient* client)
{
    TRY
    {
        return &client->GetSpace()->CreateSequencedMeshObject();
    }
    CATCH();
}

NATIVE void NativeSetSequencedMeshObjectMesh(SequencedMeshObject* object,
                                             const SpatialVertex* vertexData, const UINT vertexCount)
{
    TRY
    {
        object->SetNewMesh(vertexData, vertexCount);
    }
    CATCH();
}

NATIVE IndexedMeshObject* NativeCreateIndexedMeshObject(NativeClient* client)
{
    TRY
    {
        return &client->GetSpace()->CreateIndexedMeshObject();
    }
    CATCH();
}

NATIVE void NativeSetIndexedMeshObjectMesh(IndexedMeshObject* object,
                                           const SpatialVertex* vertexData, const UINT vertexCount,
                                           const UINT* indexData, const UINT indexCount)
{
    TRY
    {
        object->SetNewMesh(vertexData, vertexCount, indexData, indexCount);
    }
    CATCH();
}

// todo: add a way to delete mesh objects

NATIVE RasterPipeline* NativeCreateRasterPipeline(NativeClient* client,
                                                  const PipelineDescription description,
                                                  const NativeErrorMessageFunc callback)
{
    TRY
    {
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
        client->SetPostProcessingPipeline(pipeline);
    }
    CATCH();
}

NATIVE void NativeSetShaderBufferData(const ShaderBuffer* buffer, const void* data)
{
    TRY
    {
        buffer->SetData(data);
    }
    CATCH();
}

NATIVE void NativeAddDraw2DPipeline(NativeClient* client, RasterPipeline* pipeline, const draw2d::Callback callback)
{
    TRY
    {
        client->AddDraw2DPipeline(pipeline, callback);
    }
    CATCH();
}

NATIVE Texture* NativeLoadTexture(const NativeClient* client, std::byte* data, const TextureDescription description)
{
    TRY
    {
        return client->LoadTexture(data, description);
    }
    CATCH();
}

NATIVE void NativeFreeTexture(const Texture* texture)
{
    TRY
    {
        texture->Free();
    }
    CATCH();
}
