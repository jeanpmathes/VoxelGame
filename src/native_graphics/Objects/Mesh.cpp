#include "stdafx.h"
#include "Mesh.hpp"

Mesh::Mesh(NativeClient& client)
    : Drawable(client)
{
    Require(GetClient().GetDevice() != nullptr);

    instanceDataBufferAlignedSize = sizeof MeshDataBuffer;
    instanceDataBuffer            = util::AllocateConstantBuffer(GetClient(), &instanceDataBufferAlignedSize);
    NAME_D3D12_OBJECT_WITH_ID(instanceDataBuffer);

    instanceDataBufferView.BufferLocation = instanceDataBuffer.GetGPUVirtualAddress();
    instanceDataBufferView.SizeInBytes    = static_cast<UINT>(instanceDataBufferAlignedSize);

    TryDo(instanceDataBuffer.Map(&instanceConstantBufferMapping, 1));

    {
        geometrySRV.Format                  = DXGI_FORMAT_UNKNOWN;
        geometrySRV.ViewDimension           = D3D12_SRV_DIMENSION_BUFFER;
        geometrySRV.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
        geometrySRV.Buffer.FirstElement     = 0;
        geometrySRV.Buffer.Flags            = D3D12_BUFFER_SRV_FLAG_NONE;
    }

    {
        geometryUAV.Format                      = DXGI_FORMAT_UNKNOWN;
        geometryUAV.ViewDimension               = D3D12_UAV_DIMENSION_BUFFER;
        geometryUAV.Buffer.FirstElement         = 0;
        geometryUAV.Buffer.CounterOffsetInBytes = 0;
        geometryUAV.Buffer.Flags                = D3D12_BUFFER_UAV_FLAG_NONE;
    }
}

void Mesh::Initialize(UINT const materialIndex)
{
    material = &GetClient().GetSpace()->GetMaterial(materialIndex);

    Update();
}

void Mesh::Update()
{
    if (bool const isTransformDirty = ClearTransformDirty();
        !isTransformDirty)
        return;

    DirectX::XMFLOAT4X4 const objectToWorld = GetTransform();

    DirectX::XMMATRIX const transformBase   = XMLoadFloat4x4(&objectToWorld);
    DirectX::XMMATRIX const transformNormal = XMMatrixToNormal(transformBase);

    DirectX::XMFLOAT4X4 objectToWorldNormal = {};
    XMStoreFloat4x4(&objectToWorldNormal, transformNormal);

    instanceConstantBufferMapping.Write({.objectToWorld = objectToWorld, .objectToWorldNormal = objectToWorldNormal});
}

void Mesh::SetNewVertices(SpatialVertex const* vertices, UINT const vertexCount)
{
    Require(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES);
    Require(vertexCount % 4 == 0);

    UpdateGeometryViews(vertexCount, sizeof(SpatialVertex));

    if (bool const isModified = HandleModification(vertexCount);
        !isModified)
        return;
    requiresFreshBLAS = true;

    auto const vertexBufferSize = sizeof(SpatialVertex) * vertexCount;
    util::ReAllocateBuffer(&GetUploadDataBuffer(), GetClient(), vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(GetUploadDataBuffer());

    TryDo(util::MapAndWrite(GetUploadDataBuffer(), vertices, vertexCount));
}

void Mesh::SetNewBounds(SpatialBounds const* bounds, UINT const boundsCount)
{
    Require(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS);

    UpdateGeometryViews(boundsCount, sizeof(SpatialBounds));

    if (bool const isModified = HandleModification(boundsCount);
        !isModified)
        return;
    requiresFreshBLAS = true;

    auto const vertexBufferSize = sizeof(SpatialBounds) * boundsCount;
    util::ReAllocateBuffer(&GetUploadDataBuffer(), GetClient(), vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(GetUploadDataBuffer());

    TryDo(util::MapAndWrite(GetUploadDataBuffer(), bounds, boundsCount));
}

Material const& Mesh::GetMaterial() const
{
    Require(material != nullptr);
    return *material;
}

UINT Mesh::GetGeometryUnitCount() const
{
    switch (GetMaterial().geometryType)
    {
    case D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES:
        return GetDataElementCount() / 4;
    case D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS:
        return GetDataElementCount();
    default:
        throw NativeException("Unknown geometry type.");
    }
}

Allocation<ID3D12Resource> Mesh::GetGeometryBuffer() { return GeometryBuffer(); }

ShaderResources::ConstantBufferViewDescriptor Mesh::GetInstanceDataViewDescriptor() const
{
    return ShaderResources::ConstantBufferViewDescriptor(instanceDataBuffer.GetGPUVirtualAddress(), static_cast<UINT>(instanceDataBufferAlignedSize));
}

ShaderResources::ShaderResourceViewDescriptor Mesh::GetGeometryBufferViewDescriptor()
{
    return {.resource = GetGeometryBuffer(), .description = &geometrySRV};
}

ShaderResources::ShaderResourceViewDescriptor Mesh::GetAnimationSourceBufferViewDescriptor()
{
    return {.resource = sourceGeometryBuffer, .description = &geometrySRV};
}

ShaderResources::UnorderedAccessViewDescriptor Mesh::GetAnimationDestinationBufferViewDescriptor()
{
    return {.resource = destinationGeometryBuffer, .description = &geometryUAV};
}

void Mesh::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<ID3D12Resource*>* uavs, bool const isForAnimation)
{
    Require(uavs != nullptr);

    if (isForAnimation && requiresFreshBLAS) return;

    if (GetDataElementCount() == 0)
    {
        blas = {};
        return;
    }

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES) CreateBottomLevelASFromVertices(
        commandList,
        {{GeometryBuffer(), GetDataElementCount()}},
        {{usedIndexBuffer, usedIndexCount}});

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS) CreateBottomLevelASFromBounds(
        commandList,
        {{GeometryBuffer(), GetDataElementCount()}});

    if (ID3D12Resource* resource = blas.result.GetResource();
        resource != nullptr)
        uavs->push_back(resource);
}

BLAS const& Mesh::GetBLAS() { return blas; }

void Mesh::SetAnimationHandle(AnimationController::Handle const handle) { animationHandle = handle; }

AnimationController::Handle Mesh::GetAnimationHandle() const { return animationHandle; }

void Mesh::Accept(Visitor& visitor) { visitor.Visit(*this); }

void Mesh::DoDataUpload(ComPtr<ID3D12GraphicsCommandList> const& commandList, std::vector<D3D12_RESOURCE_BARRIER>* barriers)
{
    if (GetDataElementCount() == 0)
    {
        sourceGeometryBuffer      = {};
        destinationGeometryBuffer = {};
        return;
    }

    auto const geometryBufferSize = GetUploadDataBuffer().resource->GetDesc().Width;

    util::ReAllocateBuffer(&sourceGeometryBuffer, GetClient(), geometryBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_COPY_DEST, D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT_WITH_ID(sourceGeometryBuffer);

    if (GetMaterial().IsAnimated())
    {
        // A data upload will always trigger a fresh BLAS build.
        // If the mesh is not active but animated, the destination buffer will be empty.
        // Because it is inactive, the animation shader will not run and instead a copy is needed.
        bool const requiresCopy = !GetActiveIndex().has_value();

        D3D12_RESOURCE_STATES constexpr destState = D3D12_RESOURCE_STATE_COPY_DEST;
        D3D12_RESOURCE_STATES constexpr srvState  = D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;

        util::ReAllocateBuffer(
            &destinationGeometryBuffer,
            GetClient(),
            geometryBufferSize,
            D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
            requiresCopy ? destState : srvState,
            D3D12_HEAP_TYPE_DEFAULT);
        NAME_D3D12_OBJECT_WITH_ID(destinationGeometryBuffer);

        if (requiresCopy)
        {
            commandList->CopyBufferRegion(destinationGeometryBuffer.Get(), 0, GetUploadDataBuffer().Get(), 0, geometryBufferSize);

            D3D12_RESOURCE_BARRIER const transitionCopyDestToShaderResource = {CD3DX12_RESOURCE_BARRIER::Transition(destinationGeometryBuffer.Get(), destState, srvState)};
            barriers->push_back(transitionCopyDestToShaderResource);
        }
    }
    else destinationGeometryBuffer = {};

    commandList->CopyBufferRegion(sourceGeometryBuffer.Get(), 0, GetUploadDataBuffer().Get(), 0, geometryBufferSize);

    D3D12_RESOURCE_BARRIER const transitionCopyDestToShaderResource = {
        CD3DX12_RESOURCE_BARRIER::Transition(sourceGeometryBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    barriers->push_back(transitionCopyDestToShaderResource);

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES) std::tie(usedIndexBuffer, usedIndexCount) = GetClient().GetSpace()->GetIndexBuffer(
        GetDataElementCount(),
        barriers);
}

void Mesh::DoReset()
{
    material = nullptr;

    // Instance buffer is intentionally not reset, because it is reused.

    sourceGeometryBuffer      = {};
    destinationGeometryBuffer = {};

    usedIndexBuffer = {};
    usedIndexCount  = 0;

    blas              = {};
    requiresFreshBLAS = false;

    animationHandle = AnimationController::Handle::INVALID;
}

void Mesh::CreateBottomLevelASFromVertices(
    ComPtr<ID3D12GraphicsCommandList4>                           commandList,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> vertexBuffers,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> indexBuffers)
{
    if (requiresFreshBLAS)
    {
        bottomLevelASGenerator = {};

        Require(vertexBuffers.size() == indexBuffers.size());
        for (size_t index = 0; index < vertexBuffers.size(); index++)
        {
            auto const& [vertexBuffer, vertexCount] = vertexBuffers[index];
            auto const& [indexBuffer, indexCount]   = indexBuffers[index];

            bool const isOpaque = GetMaterial().isOpaque;

            bottomLevelASGenerator.AddVertexBuffer(vertexBuffer, 0, vertexCount, sizeof(SpatialVertex), indexBuffer, 0, indexCount, {}, 0, isOpaque);
        }
    }

    CreateBottomLevelAS(commandList);
}

void Mesh::CreateBottomLevelASFromBounds(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> const& boundsBuffers)
{
    if (requiresFreshBLAS)
    {
        bottomLevelASGenerator = {};

        for (auto const& [boundsBuffer, boundsCount] : boundsBuffers) bottomLevelASGenerator.AddBoundsBuffer(boundsBuffer, 0, boundsCount, sizeof(SpatialBounds));
    }

    return CreateBottomLevelAS(commandList);
}

void Mesh::CreateBottomLevelAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList)
{
    bool                      updateOnly;
    D3D12_GPU_VIRTUAL_ADDRESS previousResult;

    if (requiresFreshBLAS)
    {
        requiresFreshBLAS = false;

        UINT64     scratchSizeInBytes = 0;
        UINT64     resultSizeInBytes  = 0;
        bool const allowUpdate        = GetMaterial().IsAnimated();

        bottomLevelASGenerator.ComputeASBufferSizes(GetClient().GetDevice().Get(), allowUpdate, &scratchSizeInBytes, &resultSizeInBytes);

        blas = GetClient().GetSpace()->AllocateBLAS(resultSizeInBytes, scratchSizeInBytes);

        NAME_D3D12_OBJECT_WITH_ID(blas.scratch);
        NAME_D3D12_OBJECT_WITH_ID(blas.result);

        updateOnly     = false;
        previousResult = 0;
    }
    else
    {
        Require(GetMaterial().IsAnimated());

        updateOnly     = true;
        previousResult = blas.result.GetAddress();
    }

    bottomLevelASGenerator.Generate(commandList.Get(), blas.scratch.GetAddress(), blas.result.GetAddress(), updateOnly, previousResult);
}

Allocation<ID3D12Resource>& Mesh::GeometryBuffer()
{
    return GetMaterial().IsAnimated() ? destinationGeometryBuffer : sourceGeometryBuffer;
}

void Mesh::UpdateGeometryViews(UINT const count, UINT const stride)
{
    geometrySRV.Buffer.NumElements         = count;
    geometrySRV.Buffer.StructureByteStride = stride;

    geometryUAV.Buffer.NumElements         = count;
    geometryUAV.Buffer.StructureByteStride = stride;
}
