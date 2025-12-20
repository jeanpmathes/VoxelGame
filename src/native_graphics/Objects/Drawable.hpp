// <copyright file="Drawable.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

#include <optional>

#include "Spatial.hpp"

class Mesh;
class Effect;

/**
 * \brief Abstract base class for all drawable objects. Operations include management of modification and active state.
 */
class Drawable : public Spatial
{
    DECLARE_OBJECT_SUBCLASS(Drawable)

protected:
    explicit Drawable(NativeClient& client);

public:
    /**
     * \brief Indices into the base container of all drawables.
     */
    enum class BaseIndex : size_t
    {
    };

    /**
     * \brief Indices into the bag of entries in the drawables group.
     */
    enum class EntryIndex : size_t
    {
    };

    /**
     * \brief Indices into the bag of active drawables.
     */
    enum class ActiveIndex : size_t
    {
    };

    using BaseContainer = Bag<Drawable*, BaseIndex>;

    virtual void Update() = 0;

    /**
     * \brief Set the enabled state of this object. Only enabled objects that have data will be drawn.
     * \param enabled Whether this object should be enabled.
     */
    void SetEnabledState(bool enabled);

    /**
     * Return this object to the space. This will allow the space to reuse the object later.
     */
    void Return();

    /**
     * Enqueues commands to upload the data to the GPU.
     * Should only be called when the data is modified.
     */
    void EnqueueDataUpload(
        ComPtr<ID3D12GraphicsCommandList> const& commandList,
        std::vector<D3D12_RESOURCE_BARRIER>*     barriers);

    /**
     * Finalizes the data upload.
     * Can be called every frame, but only when all commands have been executed.
     */
    void CleanupDataUpload();

    void AssociateWithIndices(BaseIndex base, EntryIndex entry);
    void SetActiveIndex(std::optional<ActiveIndex> index);

    void Reset();

    [[nodiscard]] bool                       IsEnabled() const;
    [[nodiscard]] BaseIndex                  GetHandle() const;
    [[nodiscard]] EntryIndex                 GetEntryIndex() const;
    [[nodiscard]] std::optional<ActiveIndex> GetActiveIndex() const;
    [[nodiscard]] UINT                       GetDataElementCount() const;

    class Visitor
    {
        Visitor();

    public:
        static Visitor Empty();
        Visitor&       OnElse(std::function<void(Drawable&)> const& drawable);
        Visitor&       OnElseFail();

        void     Visit(Mesh& mesh) const;
        Visitor& OnMesh(std::function<void(Mesh&)> const& mesh);

        void     Visit(Effect& effect) const;
        Visitor& OnEffect(std::function<void(Effect&)> const& effect);

    private:
        std::function<void(Drawable&)> m_else = [](Drawable const&)
        {
            /* Default does nothing. */
        };
        std::function<void(Mesh&)>   m_mesh;
        std::function<void(Effect&)> m_effect;
    };

    virtual void Accept(Visitor& visitor) = 0;

protected:
    bool                                      HandleModification(UINT newElementCount);
    [[nodiscard]] Allocation<ID3D12Resource>& GetUploadDataBuffer();

    virtual void DoDataUpload(
        ComPtr<ID3D12GraphicsCommandList> const& commandList,
        std::vector<D3D12_RESOURCE_BARRIER>*     barriers) = 0;
    virtual void DoReset() = 0;

private:
    void UpdateActiveState();

    std::optional<BaseIndex>   m_base    = std::nullopt;
    std::optional<EntryIndex>  m_entry   = std::nullopt;
    std::optional<ActiveIndex> m_active  = std::nullopt;
    bool                       m_enabled = false;

    bool m_uploadRequired = false;
    bool m_uploadEnqueued = false;

    Allocation<ID3D12Resource> m_dataBufferUpload = {};
    UINT                       m_dataElementCount = 0;
};
