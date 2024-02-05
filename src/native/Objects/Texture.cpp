#include "stdafx.h"
#include "Texture.hpp"

namespace
{
    DXGI_FORMAT GetFormat(const ColorFormat format)
    {
        switch (format)
        {
        case RGBA:
            return DXGI_FORMAT_R8G8B8A8_UNORM;

        case BGRA:
            return DXGI_FORMAT_B8G8R8A8_UNORM;

        default:
            throw NativeException("Invalid color format.");
        }
    }

    void EnsureValidDescription(const TextureDescription& description)
    {
        REQUIRE(description.width > 0);
        REQUIRE(description.height > 0);
        REQUIRE(description.levels > 0);
    }

    constexpr auto UPLOAD_STATE = D3D12_RESOURCE_STATE_COPY_DEST;
    constexpr auto USABLE_STATE = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE |
        D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;

    Allocation<ID3D12Resource> CreateTextureResource(
        const NativeClient& client,
        const TextureDescription& description,
        const bool requiresUpload,
        D3D12_SHADER_RESOURCE_VIEW_DESC* srv)
    {
        const D3D12_RESOURCE_DESC textureDescription = CD3DX12_RESOURCE_DESC::Tex2D(
            GetFormat(description.format),
            description.width,
            description.height,
            1,
            static_cast<UINT16>(description.levels),
            1,
            0,
            D3D12_RESOURCE_FLAG_NONE);

        const auto state = requiresUpload ? UPLOAD_STATE : USABLE_STATE;

        Allocation<ID3D12Resource> texture = util::AllocateResource<ID3D12Resource>(client,
            textureDescription, D3D12_HEAP_TYPE_DEFAULT, state);
        NAME_D3D12_OBJECT(texture);

        if (srv != nullptr)
        {
            *srv = {};
            srv->Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
            srv->Format = textureDescription.Format;
            srv->ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
            srv->Texture2D.MipLevels = textureDescription.MipLevels;
        }

        return texture;
    }
}

Texture* Texture::Create(Uploader& uploader, std::byte** data, const TextureDescription description)
{
    EnsureValidDescription(description);

    D3D12_SHADER_RESOURCE_VIEW_DESC srv;
    Allocation<ID3D12Resource> texture = CreateTextureResource(uploader.GetClient(), description, true, &srv);

    uploader.UploadTexture(data, description, texture);
    
    auto result = std::make_unique<Texture>(uploader.GetClient(), texture,
                                            DirectX::XMUINT3{
                                                description.width,
                                                description.height,
                                                description.levels
                                            },
                                            srv);
    const auto ptr = result.get();

    // When uploading before use, the texture will be in safe (non-fresh) state and can be used without transition.
    ptr->m_usable = uploader.IsUploadingBeforeAnyUse();
    ptr->m_handle = uploader.GetClient().StoreObject(std::move(result));

    return ptr;
}

Texture* Texture::Create(NativeClient& client, const TextureDescription description)
{
    EnsureValidDescription(description);

    D3D12_SHADER_RESOURCE_VIEW_DESC srv;
    Allocation<ID3D12Resource> texture = CreateTextureResource(client, description, false, &srv);

    auto result = std::make_unique<Texture>(client, texture,
                                            DirectX::XMUINT3{
                                                description.width,
                                                description.height,
                                                description.levels
                                            },
                                            srv);

    const auto ptr = result.get();

    // The texture is directly created in the usable state.
    ptr->m_usable = true;
    ptr->m_handle = client.StoreObject(std::move(result));

    return ptr;
}

Texture::Texture(
    NativeClient& client,
    const Allocation<ID3D12Resource>& resource,
    DirectX::XMUINT3 size,
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc)
    : Object(client), m_resource(resource), m_srvDesc(srvDesc), m_size(size), m_usable(false)
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

DirectX::XMUINT3 Texture::GetSize() const
{
    return m_size;
}

void Texture::TransitionToUsable(const ComPtr<ID3D12GraphicsCommandList> commandList)
{
    if (m_usable) return;

    CreateUsabilityBarrier(commandList, m_resource);

    m_usable = true;
}

void Texture::CreateUsabilityBarrier(
    const ComPtr<ID3D12GraphicsCommandList> commandList,
    const Allocation<ID3D12Resource> resource)
{
    const CD3DX12_RESOURCE_BARRIER barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        resource.Get(),
        UPLOAD_STATE,
        USABLE_STATE);

    commandList->ResourceBarrier(1, &barrier);
}
