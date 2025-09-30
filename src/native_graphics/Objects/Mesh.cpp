#include "stdafx.h"
#include "Mesh.hpp"

Mesh::Mesh(NativeClient& client)
    : Drawable(client)
{
    Require(GetClient().GetDevice() != nullptr);

    m_instanceDataBufferAlignedSize = sizeof MeshDataBuffer;
    m_instanceDataBuffer            = util::AllocateConstantBuffer(GetClient(), &m_instanceDataBufferAlignedSize);
    NAME_D3D12_OBJECT_WITH_ID(m_instanceDataBuffer);

    m_instanceDataBufferView.BufferLocation = m_instanceDataBuffer.GetGPUVirtualAddress();
    m_instanceDataBufferView.SizeInBytes    = static_cast<UINT>(m_instanceDataBufferAlignedSize);

    TryDo(m_instanceDataBuffer.Map(&m_instanceConstantBufferMapping, 1));

    {
        m_geometrySRV.Format                  = DXGI_FORMAT_UNKNOWN;
        m_geometrySRV.ViewDimension           = D3D12_SRV_DIMENSION_BUFFER;
        m_geometrySRV.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
        m_geometrySRV.Buffer.FirstElement     = 0;
        m_geometrySRV.Buffer.Flags            = D3D12_BUFFER_SRV_FLAG_NONE;
    }

    {
        m_geometryUAV.Format                      = DXGI_FORMAT_UNKNOWN;
        m_geometryUAV.ViewDimension               = D3D12_UAV_DIMENSION_BUFFER;
        m_geometryUAV.Buffer.FirstElement         = 0;
        m_geometryUAV.Buffer.CounterOffsetInBytes = 0;
        m_geometryUAV.Buffer.Flags                = D3D12_BUFFER_UAV_FLAG_NONE;
    }
}

void Mesh::Initialize(UINT const materialIndex)
{
    m_material = &GetClient().GetSpace()->GetMaterial(materialIndex);

    Update();
}

void Mesh::Update()
{
    if (bool const transformDirty = ClearTransformDirty();
        !transformDirty)
        return;

    DirectX::XMFLOAT4X4 const objectToWorld = GetTransform();

    DirectX::XMMATRIX const transform       = XMLoadFloat4x4(&objectToWorld);
    DirectX::XMMATRIX const transformNormal = XMMatrixToNormal(transform);

    DirectX::XMFLOAT4X4 objectToWorldNormal = {};
    XMStoreFloat4x4(&objectToWorldNormal, transformNormal);

    m_instanceConstantBufferMapping.Write({.objectToWorld = objectToWorld, .objectToWorldNormal = objectToWorldNormal});
}

void Mesh::SetNewVertices(SpatialVertex const* vertices, UINT const vertexCount)
{
    Require(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES);
    Require(vertexCount % 4 == 0);

    UpdateGeometryViews(vertexCount, sizeof(SpatialVertex));

    if (bool const uploadRequired = HandleModification(vertexCount);
        !uploadRequired)
        return;
    m_requiresFreshBLAS = true;

    auto const vertexBufferSize = sizeof(SpatialVertex) * vertexCount;
    util::ReAllocateBuffer(&GetUploadDataBuffer(), GetClient(), vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(GetUploadDataBuffer());

    TryDo(util::MapAndWrite(GetUploadDataBuffer(), vertices, vertexCount));
}

void Mesh::SetNewBounds(SpatialBounds const* bounds, UINT const boundsCount)
{
    Require(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS);

    UpdateGeometryViews(boundsCount, sizeof(SpatialBounds));

    if (bool const uploadRequired = HandleModification(boundsCount);
        !uploadRequired)
        return;
    m_requiresFreshBLAS = true;

    auto const vertexBufferSize = sizeof(SpatialBounds) * boundsCount;
    util::ReAllocateBuffer(&GetUploadDataBuffer(), GetClient(), vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(GetUploadDataBuffer());

    TryDo(util::MapAndWrite(GetUploadDataBuffer(), bounds, boundsCount));
}

Material const& Mesh::GetMaterial() const
{
    Require(m_material != nullptr);
    return *m_material;
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
    return ShaderResources::ConstantBufferViewDescriptor(m_instanceDataBuffer.GetGPUVirtualAddress(), static_cast<UINT>(m_instanceDataBufferAlignedSize));
}

ShaderResources::ShaderResourceViewDescriptor Mesh::GetGeometryBufferViewDescriptor() { return {.resource = GetGeometryBuffer(), .description = &m_geometrySRV}; }

ShaderResources::ShaderResourceViewDescriptor Mesh::GetAnimationSourceBufferViewDescriptor() { return {.resource = m_sourceGeometryBuffer, .description = &m_geometrySRV}; }

ShaderResources::UnorderedAccessViewDescriptor Mesh::GetAnimationDestinationBufferViewDescriptor()
{
    return {.resource = m_destinationGeometryBuffer, .description = &m_geometryUAV};
}

void Mesh::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<ID3D12Resource*>* uavs, bool const isForAnimation)
{
    Require(uavs != nullptr);

    if (isForAnimation && m_requiresFreshBLAS) return;

    if (GetDataElementCount() == 0)
    {
        m_blas = {};
        return;
    }

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
        CreateBottomLevelASFromVertices(commandList, {{GeometryBuffer(), GetDataElementCount()}}, {{m_usedIndexBuffer, m_usedIndexCount}});

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS)
        CreateBottomLevelASFromBounds(commandList, {{GeometryBuffer(), GetDataElementCount()}});

    if (ID3D12Resource* resource = m_blas.result.GetResource();
        resource != nullptr)
        uavs->push_back(resource);
}

BLAS const& Mesh::GetBLAS() { return m_blas; }

void Mesh::SetAnimationHandle(AnimationController::Handle const handle) { m_animationHandle = handle; }

AnimationController::Handle Mesh::GetAnimationHandle() const { return m_animationHandle; }

void Mesh::Accept(Visitor& visitor) { visitor.Visit(*this); }

void Mesh::DoDataUpload(ComPtr<ID3D12GraphicsCommandList> const& commandList, std::vector<D3D12_RESOURCE_BARRIER>* barriers)
{
    if (GetDataElementCount() == 0)
    {
        m_sourceGeometryBuffer      = {};
        m_destinationGeometryBuffer = {};
        return;
    }

    auto const geometryBufferSize = GetUploadDataBuffer().resource->GetDesc().Width;

    util::ReAllocateBuffer(&m_sourceGeometryBuffer, GetClient(), geometryBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_COPY_DEST, D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT_WITH_ID(m_sourceGeometryBuffer);

    if (GetMaterial().IsAnimated())
    {
        // A data upload will always trigger a fresh BLAS build.
        // If the mesh is not active but animated, the destination buffer will be empty.
        // Because it is inactive, the animation shader will not run and instead a copy is needed.
        bool const requiresCopy = !GetActiveIndex().has_value();

        D3D12_RESOURCE_STATES constexpr destState = D3D12_RESOURCE_STATE_COPY_DEST;
        D3D12_RESOURCE_STATES constexpr srvState  = D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;

        util::ReAllocateBuffer(
            &m_destinationGeometryBuffer,
            GetClient(),
            geometryBufferSize,
            D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
            requiresCopy ? destState : srvState,
            D3D12_HEAP_TYPE_DEFAULT);
        NAME_D3D12_OBJECT_WITH_ID(m_destinationGeometryBuffer);

        if (requiresCopy)
        {
            commandList->CopyBufferRegion(m_destinationGeometryBuffer.Get(), 0, GetUploadDataBuffer().Get(), 0, geometryBufferSize);

            D3D12_RESOURCE_BARRIER const transitionCopyDestToShaderResource = {CD3DX12_RESOURCE_BARRIER::Transition(m_destinationGeometryBuffer.Get(), destState, srvState)};
            barriers->push_back(transitionCopyDestToShaderResource);
        }
    }
    else m_destinationGeometryBuffer = {};

    commandList->CopyBufferRegion(m_sourceGeometryBuffer.Get(), 0, GetUploadDataBuffer().Get(), 0, geometryBufferSize);

    D3D12_RESOURCE_BARRIER const transitionCopyDestToShaderResource = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_sourceGeometryBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    barriers->push_back(transitionCopyDestToShaderResource);

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
        std::tie(m_usedIndexBuffer, m_usedIndexCount) = GetClient().GetSpace()->GetIndexBuffer(GetDataElementCount(), barriers);
}

void Mesh::DoReset()
{
    m_material = nullptr;

    // Instance buffer is intentionally not reset, because it is reused.

    m_sourceGeometryBuffer      = {};
    m_destinationGeometryBuffer = {};

    m_usedIndexBuffer = {};
    m_usedIndexCount  = 0;

    m_blas              = {};
    m_requiresFreshBLAS = false;

    m_animationHandle = AnimationController::Handle::INVALID;
}

void Mesh::CreateBottomLevelASFromVertices(
    ComPtr<ID3D12GraphicsCommandList4>                           commandList,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> vertexBuffers,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> indexBuffers)
{
    if (m_requiresFreshBLAS)
    {
        m_bottomLevelASGenerator = {};

        Require(vertexBuffers.size() == indexBuffers.size());
        for (size_t index = 0; index < vertexBuffers.size(); index++)
        {
            auto const& [vertexBuffer, vertexCount] = vertexBuffers[index];
            auto const& [indexBuffer, indexCount]   = indexBuffers[index];

            bool const isOpaque = GetMaterial().isOpaque;

            m_bottomLevelASGenerator.AddVertexBuffer(vertexBuffer, 0, vertexCount, sizeof(SpatialVertex), indexBuffer, 0, indexCount, {}, 0, isOpaque);
        }
    }

    CreateBottomLevelAS(commandList);
}

void Mesh::CreateBottomLevelASFromBounds(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> const& boundsBuffers)
{
    if (m_requiresFreshBLAS)
    {
        m_bottomLevelASGenerator = {};

        for (auto const& [boundsBuffer, boundsCount] : boundsBuffers)
            m_bottomLevelASGenerator.AddBoundsBuffer(boundsBuffer, 0, boundsCount, sizeof(SpatialBounds));
    }

    return CreateBottomLevelAS(commandList);
}

void Mesh::CreateBottomLevelAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList)
{
    bool                      updateOnly;
    D3D12_GPU_VIRTUAL_ADDRESS previousResult;

    if (m_requiresFreshBLAS)
    {
        m_requiresFreshBLAS = false;

        UINT64     scratchSizeInBytes = 0;
        UINT64     resultSizeInBytes  = 0;
        bool const allowUpdate        = GetMaterial().IsAnimated();

        m_bottomLevelASGenerator.ComputeASBufferSizes(GetClient().GetDevice().Get(), allowUpdate, &scratchSizeInBytes, &resultSizeInBytes);

        m_blas = GetClient().GetSpace()->AllocateBLAS(resultSizeInBytes, scratchSizeInBytes);

        NAME_D3D12_OBJECT_WITH_ID(m_blas.scratch);
        NAME_D3D12_OBJECT_WITH_ID(m_blas.result);

        updateOnly     = false;
        previousResult = 0;
    }
    else
    {
        Require(GetMaterial().IsAnimated());

        updateOnly     = true;
        previousResult = m_blas.result.GetAddress();
    }

    m_bottomLevelASGenerator.Generate(commandList.Get(), m_blas.scratch.GetAddress(), m_blas.result.GetAddress(), updateOnly, previousResult);
}

Allocation<ID3D12Resource>& Mesh::GeometryBuffer() { return GetMaterial().IsAnimated() ? m_destinationGeometryBuffer : m_sourceGeometryBuffer; }

void Mesh::UpdateGeometryViews(UINT const count, UINT const stride)
{
    m_geometrySRV.Buffer.NumElements         = count;
    m_geometrySRV.Buffer.StructureByteStride = stride;

    m_geometryUAV.Buffer.NumElements         = count;
    m_geometryUAV.Buffer.StructureByteStride = stride;
}
