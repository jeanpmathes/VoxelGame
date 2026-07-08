// <copyright file="DebugLayer.cpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

#include "stdafx.h"

DebugLayer::Filter::Filter(DebugLayer* const layer, bool const active)
    : layer(layer)
  , active(active)
{
}

DebugLayer::Filter::~Filter()
{
    Reset();
}

DebugLayer::Filter::Filter(Filter&& other) noexcept
    : layer(other.layer)
  , active(other.active)
{
    other.layer  = nullptr;
    other.active = false;
}

DebugLayer::Filter& DebugLayer::Filter::operator=(Filter&& other) noexcept
{
    if (this == &other) return *this;

    Reset();

    layer  = other.layer;
    active = other.active;

    other.layer  = nullptr;
    other.active = false;

    return *this;
}

void DebugLayer::Filter::Reset()
{
    if (!active) return;

    layer->PopFilterImplementation();
    active = false;
    layer  = nullptr;
}

DebugLayer::DebugLayer(D3D12MessageFunc const callback)
    : callback(callback)
{
}

DebugLayer::~DebugLayer()
{
    UnregisterCallback();
}

void DebugLayer::UnregisterCallback()
{
    if (infoQueue == nullptr || callbackCookie == 0) return;

    // Do not use TryDo as we do not want the destructor to throw.
    (void)infoQueue->UnregisterMessageCallback(callbackCookie);

    callbackCookie = 0;
}

void DebugLayer::ConfigureDeviceFactoryImplementation(ComPtr<ID3D12DeviceFactory> const& deviceFactory, NativeClient const& client) const
{
    (void)this;

    ComPtr<ID3D12Debug5> debug;
    if (SUCCEEDED(deviceFactory->GetConfigurationInterface(CLSID_D3D12Debug, IID_PPV_ARGS(&debug))))
    {
        debug->EnableDebugLayer();
        debug->SetEnableAutoName(TRUE);

        if (!client.SupportPIX() && client.UseGBV()) debug->SetEnableGPUBasedValidation(TRUE);
    }

    ComPtr<ID3D12DeviceRemovedExtendedDataSettings1> dredSettings;
    if (SUCCEEDED(deviceFactory->GetConfigurationInterface(CLSID_D3D12DeviceRemovedExtendedData, IID_PPV_ARGS(&dredSettings))))
    {
        dredSettings->SetAutoBreadcrumbsEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
        dredSettings->SetPageFaultEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
        dredSettings->SetBreadcrumbContextEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
    }
}

void DebugLayer::ConfigureDeviceImplementation(ComPtr<ID3D12Device5> const& device, NativeClient const& client, bool const pixAttached)
{
    auto wrappedCallback = [](
        D3D12_MESSAGE_CATEGORY const category,
        D3D12_MESSAGE_SEVERITY const severity,
        D3D12_MESSAGE_ID const       id,
        LPCSTR const                 description,
        void* const                  context) -> void
    {
        auto const self = static_cast<DebugLayer*>(context);

        Win32Application::EnterErrorMode();
        self->InvokeCallback(category, severity, id, description);
        Win32Application::ExitErrorMode();
    };

    HRESULT const infoQueueResult = device.As(&infoQueue);
    if (SUCCEEDED(infoQueueResult))
    {
        {
            D3D12_INFO_QUEUE_FILTER filter = {};

            // The denied IDs are spammy and of little value, so we supress them.

            std::vector deniedIDs = {
                D3D12_MESSAGE_ID_CREATE_COMMANDQUEUE,
                D3D12_MESSAGE_ID_CREATE_COMMANDALLOCATOR,
                D3D12_MESSAGE_ID_CREATE_PIPELINESTATE,
                D3D12_MESSAGE_ID_CREATE_COMMANDLIST12,
                D3D12_MESSAGE_ID_CREATE_RESOURCE,
                D3D12_MESSAGE_ID_CREATE_DESCRIPTORHEAP,
                D3D12_MESSAGE_ID_CREATE_ROOTSIGNATURE,
                D3D12_MESSAGE_ID_CREATE_LIBRARY,
                D3D12_MESSAGE_ID_CREATE_HEAP,
                D3D12_MESSAGE_ID_CREATE_MONITOREDFENCE,
                D3D12_MESSAGE_ID_CREATE_QUERYHEAP,
                D3D12_MESSAGE_ID_CREATE_COMMANDSIGNATURE,
                D3D12_MESSAGE_ID_CREATE_LIFETIMETRACKER,
                D3D12_MESSAGE_ID_CREATE_SHADERCACHESESSION,

                D3D12_MESSAGE_ID_DESTROY_COMMANDQUEUE,
                D3D12_MESSAGE_ID_DESTROY_COMMANDALLOCATOR,
                D3D12_MESSAGE_ID_DESTROY_PIPELINESTATE,
                D3D12_MESSAGE_ID_DESTROY_COMMANDLIST12,
                D3D12_MESSAGE_ID_DESTROY_RESOURCE,
                D3D12_MESSAGE_ID_DESTROY_DESCRIPTORHEAP,
                D3D12_MESSAGE_ID_DESTROY_ROOTSIGNATURE,
                D3D12_MESSAGE_ID_DESTROY_LIBRARY,
                D3D12_MESSAGE_ID_DESTROY_HEAP,
                D3D12_MESSAGE_ID_DESTROY_MONITOREDFENCE,
                D3D12_MESSAGE_ID_DESTROY_QUERYHEAP,
                D3D12_MESSAGE_ID_DESTROY_COMMANDSIGNATURE,
                D3D12_MESSAGE_ID_DESTROY_LIFETIMETRACKER,
                D3D12_MESSAGE_ID_DESTROY_SHADERCACHESESSION
            };

            filter.DenyList.NumIDs  = static_cast<UINT>(deniedIDs.size());
            filter.DenyList.pIDList = deniedIDs.data();

            TryDo(infoQueue->PushStorageFilter(&filter));
        }

        TryDo(infoQueue->RegisterMessageCallback(wrappedCallback, D3D12_MESSAGE_CALLBACK_FLAG_NONE, this, &callbackCookie));

        AddMessage("Installed debug callback");

        if (pixAttached && !client.SupportPIX()) AddWarning("PIX detected, consider using the --pix command line argument");
    }
    else InvokeCallback(D3D12_MESSAGE_CATEGORY_APPLICATION_DEFINED, D3D12_MESSAGE_SEVERITY_WARNING, D3D12_MESSAGE_ID_UNKNOWN, "Failed to install debug callback");
}

void DebugLayer::AddMessageImplementation(D3D12_MESSAGE_SEVERITY const severity, char const* const message) const
{
    if (infoQueue == nullptr) return;

    TryDo(infoQueue->AddApplicationMessage(severity, message));
}

DebugLayer::Filter DebugLayer::PushFilterImplementation(D3D12_MESSAGE_ID const id)
{
    D3D12_INFO_QUEUE_FILTER filter     = {};
    D3D12_MESSAGE_ID        filteredId = id;

    filter.DenyList.NumIDs  = 1;
    filter.DenyList.pIDList = &filteredId;

    // Only the top-most filter of the filter stack is used, so we copy our default filter.

    TryDo(infoQueue->PushCopyOfStorageFilter());
    TryDo(infoQueue->AddStorageFilterEntries(&filter));

    return {this, true};
}

void DebugLayer::PopFilterImplementation() const
{
    if (infoQueue == nullptr) return;

    infoQueue->PopStorageFilter();
}

void DebugLayer::InvokeCallback(D3D12_MESSAGE_CATEGORY const category, D3D12_MESSAGE_SEVERITY const severity, D3D12_MESSAGE_ID const id, char const* const description) const
{
    if (callback == nullptr) return;

    callback(category, severity, id, description, nullptr);
}
