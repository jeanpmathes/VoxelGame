// <copyright file="Context.hpp" company="VoxelGame">
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

class Context;

namespace ui
{
    /**
     * \brief Context in which any UI is rendered, owning main Direct2D and DirectWrite resources.
     */
    class Context final
    {
    public:
        explicit Context(::Context& context);
        ~Context() = default;

        Context(Context const&) = delete;
        Context(Context&&)      = delete;

        Context& operator=(Context const&) = delete;
        Context& operator=(Context&&)      = delete;

        void ReleaseRenderTargets();
        void CreateRenderTargets();

        void BeginFrame(UINT frameIndex) const;
        void EndFrame(UINT frameIndex) const;

        [[nodiscard]] IDWriteFactory* GetDirectWriteFactory() const;

    private:
        std::vector<ID3D11Resource*> GetWrappedResources(UINT frameIndex) const;

        ::Context& context;

        ComPtr<ID2D1Factory3>      direct2DFactory;
        ComPtr<ID2D1Device2>       direct2DDevice;
        ComPtr<ID2D1DeviceContext> direct2DDeviceContext;
        ComPtr<IDWriteFactory>     directWriteFactory;

        struct RenderTarget
        {
            ComPtr<ID3D11Resource> wrappedBackBuffer;
            ComPtr<IDXGISurface>   surface;
            ComPtr<ID2D1Bitmap1>   bitmap;
        };

        std::array<RenderTarget, FRAME_COUNT> renderTargets;

        FLOAT dpi = 96;
    };
}
