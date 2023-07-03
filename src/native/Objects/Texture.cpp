#include "stdafx.h"
#include "Texture.h"

Texture* Texture::Create(Uploader& uploader, std::byte** data, TextureDescription description)
{
    REQUIRE(description.width > 0);
    REQUIRE(description.height > 0);
    REQUIRE(description.depth > 0);

    REQUIRE(description.depth < D3D12_REQ_TEXTURE2D_ARRAY_AXIS_DIMENSION);

    const D3D12_RESOURCE_DESC textureDesc = CD3DX12_RESOURCE_DESC::Tex2D(
        DXGI_FORMAT_B8G8R8A8_UNORM,
        description.width,
        description.height,
        static_cast<UINT16>(description.depth),
        1,
        1,
        0,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS);
    
    Allocation<ID3D12Resource> texture = util::AllocateResource<ID3D12Resource>(uploader.GetClient(),
        textureDesc, D3D12_HEAP_TYPE_DEFAULT, D3D12_RESOURCE_STATE_COPY_DEST);

    NAME_D3D12_OBJECT(texture);

    UINT subresources = description.depth;
    uploader.UploadTexture(data, subresources, description, texture);

    D3D12_SRV_DIMENSION dimension = D3D12_SRV_DIMENSION_TEXTURE2D;
    if (description.depth > 1)
    {
        dimension = D3D12_SRV_DIMENSION_TEXTURE2DARRAY;
    }

    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDesc.Format = textureDesc.Format;
    srvDesc.ViewDimension = dimension;
    srvDesc.Texture2D.MipLevels = textureDesc.MipLevels;

    auto result = std::make_unique<Texture>(uploader.GetClient(), texture, srvDesc);
    auto ptr = result.get();

    // With an individual upload, the texture will be in safe (non-fresh) state and can be used without transition.
    ptr->m_usable = !uploader.IsUploadingIndividually();
    ptr->m_handle = uploader.GetClient().StoreObject(std::move(result));

    return ptr;
}

Texture::Texture(NativeClient& client, const Allocation<ID3D12Resource> resource,
                 D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc)
    : Object(client), m_resource(resource), m_srvDesc(srvDesc), m_usable(true)
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

D3D12_SHADER_RESOURCE_VIEW_DESC Texture::GetView() const
{
    return m_srvDesc;
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
