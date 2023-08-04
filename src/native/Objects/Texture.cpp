#include "stdafx.h"
#include "Texture.hpp"

Texture* Texture::Create(Uploader& uploader, std::byte** data, TextureDescription description)
{
    REQUIRE(description.width > 0);
    REQUIRE(description.height > 0);
    REQUIRE(description.mipLevels > 0);

    const D3D12_RESOURCE_DESC textureDescription = CD3DX12_RESOURCE_DESC::Tex2D(
        DXGI_FORMAT_B8G8R8A8_UNORM,
        description.width,
        description.height,
        1,
        static_cast<UINT16>(description.mipLevels),
        1,
        0,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS);

    Allocation<ID3D12Resource> texture = util::AllocateResource<ID3D12Resource>(uploader.GetClient(),
        textureDescription, D3D12_HEAP_TYPE_DEFAULT, D3D12_RESOURCE_STATE_COPY_DEST);

    NAME_D3D12_OBJECT(texture);

    uploader.UploadTexture(data, description, texture);

    D3D12_SRV_DIMENSION dimension = D3D12_SRV_DIMENSION_TEXTURE2D;

    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDesc.Format = textureDescription.Format;
    srvDesc.ViewDimension = dimension;
    srvDesc.Texture2D.MipLevels = textureDescription.MipLevels;


    auto result = std::make_unique<Texture>(uploader.GetClient(), texture,
                                            DirectX::XMUINT2{description.width, description.height}, srvDesc);
    auto ptr = result.get();

    // With an individual upload, the texture will be in safe (non-fresh) state and can be used without transition.
    ptr->m_usable = !uploader.IsUploadingIndividually();
    ptr->m_handle = uploader.GetClient().StoreObject(std::move(result));

    return ptr;
}

Texture::Texture(
    NativeClient& client,
    const Allocation<ID3D12Resource> resource,
    DirectX::XMUINT2 size,
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc)
    : Object(client), m_resource(resource), m_srvDesc(srvDesc), m_size(size), m_usable(true)
{
    NAME_D3D12_OBJECT_WITH_ID(m_resource);
}

void Texture::Free() const
{
    GetClient().DeleteObject(m_handle);
}

Allocation<ID3D12Resource> Texture::GetResource() const
{
    return m_resource;
}

const D3D12_SHADER_RESOURCE_VIEW_DESC& Texture::GetView() const
{
    return m_srvDesc;
}

DirectX::XMUINT2 Texture::GetSize() const
{
    return m_size;
}

void Texture::TransitionToUsable(const ComPtr<ID3D12GraphicsCommandList> commandList)
{
    if (!m_usable) return;

    CreateUsabilityBarrier(commandList, m_resource);

    m_usable = false;
}

void Texture::CreateUsabilityBarrier(
    const ComPtr<ID3D12GraphicsCommandList> commandList, const Allocation<ID3D12Resource> resource)
{
    const CD3DX12_RESOURCE_BARRIER barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        resource.Get(),
        D3D12_RESOURCE_STATE_COPY_DEST,
        D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);

    commandList->ResourceBarrier(1, &barrier);
}
