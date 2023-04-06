﻿#include "stdafx.h"
#include "Texture.h"

std::unique_ptr<Texture> Texture::Create(Uploader& uploader, std::byte* data, TextureDescription description)
{
    const D3D12_RESOURCE_DESC textureDesc = CD3DX12_RESOURCE_DESC::Tex2D(
        DXGI_FORMAT_B8G8R8A8_UNORM,
        description.width,
        description.height,
        1,
        1,
        1,
        0,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS);

    const CD3DX12_HEAP_PROPERTIES textureProperties(D3D12_HEAP_TYPE_DEFAULT);

    ComPtr<ID3D12Resource> texture;

    TRY_DO(uploader.GetDevice()->CreateCommittedResource(
        &textureProperties,
        D3D12_HEAP_FLAG_NONE,
        &textureDesc,
        D3D12_RESOURCE_STATE_COPY_DEST,
        nullptr,
        IID_PPV_ARGS(&texture)));

    uploader.UploadTexture(data, 0, 1, description, texture);

    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDesc.Format = textureDesc.Format;
    srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Texture2D.MipLevels = textureDesc.MipLevels;

    return std::make_unique<Texture>(uploader.GetClient(), texture, srvDesc);
}

Texture::Texture(NativeClient& client, const ComPtr<ID3D12Resource> resource, D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc)
    : Object(client), m_resource(resource), m_srvDesc(srvDesc)
{
    NAME_D3D12_OBJECT_WITH_ID(resource);
}

ComPtr<ID3D12Resource> Texture::GetResource() const
{
    return m_resource;
}

D3D12_SHADER_RESOURCE_VIEW_DESC Texture::GetView() const
{
    return m_srvDesc;
}
