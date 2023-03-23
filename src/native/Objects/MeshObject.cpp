#include "stdafx.h"
#include "MeshObject.h"

MeshObject::MeshObject(NativeClient& client) : SpatialObject(client)
{
    assert(GetClient().GetDevice() != nullptr);

    m_instanceConstantBuffer = nv_helpers_dx12::CreateBuffer(GetClient().GetDevice().Get(),
                                                             sizeof m_instanceConstantBufferData,
                                                             D3D12_RESOURCE_FLAG_NONE,
                                                             D3D12_RESOURCE_STATE_GENERIC_READ,
                                                             nv_helpers_dx12::kUploadHeapProps);

    Update();
}

void MeshObject::Update()
{
    const DirectX::XMFLOAT4X4 objectToWorld = GetTransform();

    const DirectX::XMMATRIX transform = XMLoadFloat4x4(&objectToWorld);
    const DirectX::XMMATRIX transformNormal = XMMatrixToNormal(transform);

    DirectX::XMFLOAT4X4 objectToWorldNormal = {};
    XMStoreFloat4x4(&objectToWorldNormal, transformNormal);

    m_instanceConstantBufferData = {
        .objectToWorld = objectToWorld,
        .objectToWorldNormal = objectToWorldNormal
    };

    uint8_t* pData;
    TRY_DO(m_instanceConstantBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

    memcpy(pData, &m_instanceConstantBufferData, sizeof m_instanceConstantBufferData);

    m_instanceConstantBuffer->Unmap(0, nullptr);
}

void MeshObject::FillArguments(StandardShaderArguments& shaderArguments) const
{
    shaderArguments.instanceBuffer = reinterpret_cast<void*>(m_instanceConstantBuffer->GetGPUVirtualAddress());
}

AccelerationStructureBuffers MeshObject::CreateBottomLevelAS(ComPtr<ID3D12GraphicsCommandList4> commandList,
                                                             std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>>
                                                             vertexBuffers,
                                                             std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>>
                                                             indexBuffers) const
{
    nv_helpers_dx12::BottomLevelASGenerator bottomLevelAS;

    for (size_t i = 0; i < vertexBuffers.size(); i++)
    {
        auto& [buffer, count] = vertexBuffers[i];

        if (auto [indexBuffer, indexCount] = i < indexBuffers.size() ? indexBuffers[i] : std::make_pair(nullptr, 0);
            indexCount > 0)
        {
            bottomLevelAS.AddVertexBuffer(buffer.Get(), 0, count, sizeof(SpatialVertex),
                                          indexBuffer.Get(), 0, indexCount, nullptr, 0, true);
        }
        else
        {
            bottomLevelAS.AddVertexBuffer(buffer.Get(), 0, count, sizeof(SpatialVertex), nullptr, 0);
        }
    }

    UINT64 scratchSizeInBytes = 0;
    UINT64 resultSizeInBytes = 0;
    bottomLevelAS.ComputeASBufferSizes(GetClient().GetDevice().Get(), false, &scratchSizeInBytes, &resultSizeInBytes);

    AccelerationStructureBuffers buffers;
    buffers.scratch = nv_helpers_dx12::CreateBuffer(GetClient().GetDevice().Get(), scratchSizeInBytes,
                                                    D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                    D3D12_RESOURCE_STATE_COMMON,
                                                    nv_helpers_dx12::kDefaultHeapProps);
    buffers.result = nv_helpers_dx12::CreateBuffer(GetClient().GetDevice().Get(), resultSizeInBytes,
                                                   D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                   D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
                                                   nv_helpers_dx12::kDefaultHeapProps);

    bottomLevelAS.Generate(commandList.Get(), buffers.scratch.Get(), buffers.result.Get(),
                           false, nullptr);
    return buffers;
}
