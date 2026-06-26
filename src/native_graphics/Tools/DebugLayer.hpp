// <copyright file="DebugLayer.hpp" company="VoxelGame">
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

#pragma once

class NativeClient;

/**
 * \brief Owns DirectX debug layer configuration and message reporting.
 */
class DebugLayer final
{
public:
    class Filter final
    {
    public:
        Filter() = default;
        ~Filter();

        Filter(Filter const&)            = delete;
        Filter& operator=(Filter const&) = delete;

        Filter(Filter&& other) noexcept;
        Filter& operator=(Filter&& other) noexcept;

    private:
        friend class DebugLayer;

        Filter(DebugLayer* layer, bool active);

        void Reset();

        DebugLayer* layer  = nullptr;
        bool        active = false;
    };

    explicit DebugLayer(D3D12MessageFunc callback);
    ~DebugLayer();

    [[nodiscard]] UINT GetDXGIFactoryFlags() const
    {
        (void)this;

#ifdef NATIVE_DEBUG
        return DXGI_CREATE_FACTORY_DEBUG;
#else
        return 0;
#endif
    }

    void ConfigureDeviceFactory(ComPtr<ID3D12DeviceFactory> const& deviceFactory, NativeClient const& client) const
    {
#ifdef NATIVE_DEBUG
        ConfigureDeviceFactoryImplementation(deviceFactory, client);
#else
        (void)deviceFactory;
        (void)client;
#endif
    }

    void ConfigureDevice(ComPtr<ID3D12Device5> const& device, NativeClient const& client)
    {
#ifdef NATIVE_DEBUG
        ConfigureDeviceImplementation(device, client, PIXIsAttachedForGpuCapture());
#else
        (void)device;
        (void)client;
#endif
    }

    void AddMessage(char const* message) const
    {
#ifdef NATIVE_DEBUG
        AddMessageImplementation(D3D12_MESSAGE_SEVERITY_MESSAGE, message);
#else
        (void)message;
#endif
    }

    void AddWarning(char const* message) const
    {
#ifdef NATIVE_DEBUG
        AddMessageImplementation(D3D12_MESSAGE_SEVERITY_WARNING, message);
#else
        (void)message;
#endif
    }

    void AddInfo(char const* message) const
    {
#ifdef NATIVE_DEBUG
        AddMessageImplementation(D3D12_MESSAGE_SEVERITY_INFO, message);
#else
        (void)message;
#endif
    }

    void AddError(char const* message) const
    {
#ifdef NATIVE_DEBUG
        AddMessageImplementation(D3D12_MESSAGE_SEVERITY_ERROR, message);
#else
        (void)message;
#endif
    }

    [[nodiscard]] Filter PushFilter(D3D12_MESSAGE_ID const id)
    {
#ifdef NATIVE_DEBUG
        return PushFilterImplementation(id);
#else
        (void)id;
        return {};
#endif
    }

private:
    void UnregisterCallback();

    void ConfigureDeviceFactoryImplementation(ComPtr<ID3D12DeviceFactory> const& deviceFactory, NativeClient const& client) const;
    void ConfigureDeviceImplementation(ComPtr<ID3D12Device5> const& device, NativeClient const& client, bool pixAttached);
    void AddMessageImplementation(D3D12_MESSAGE_SEVERITY severity, char const* message) const;

    Filter PushFilterImplementation(D3D12_MESSAGE_ID id);
    void   PopFilterImplementation() const;

    void InvokeCallback(D3D12_MESSAGE_CATEGORY category, D3D12_MESSAGE_SEVERITY severity, D3D12_MESSAGE_ID id, char const* description) const;

    D3D12MessageFunc         callback;
    ComPtr<ID3D12InfoQueue1> infoQueue;
    DWORD                    callbackCookie = 0;
};
