// <copyright file="DrawablesGroup.hpp" company="VoxelGame">
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

#include <ranges>

#include "Objects/Drawable.hpp"

/**
 * \brief Base class for all drawable groups, offering common functionality.
 */
class Drawables
{
public:
    Drawables() = default;

    Drawables(Drawables const&)            = delete;
    Drawables& operator=(Drawables const&) = delete;
    Drawables(Drawables&&)                 = delete;
    Drawables& operator=(Drawables&&)      = delete;

    virtual ~Drawables() = default;

    /**
     * \brief Enqueue the data upload for all modified drawables.
     */
    virtual void EnqueueDataUpload(ComPtr<ID3D12GraphicsCommandList4> commandList) = 0;

    /**
     * \brief Cleanup the data upload resources after performing the upload.
     */
    virtual void CleanupDataUpload() = 0;
};

/**
 * \brief A group of drawables that have the same subtype.
 * \tparam D The subtype of the drawables.
 */
template <class D>
    requires std::is_base_of_v<Drawable, D>
class DrawablesGroup final : public Drawables
{
public:
    /**
     * \brief Creates a new drawables group.
     * \param client The native client, used for creating new drawables.
     * \param common A common bag of drawables of all subtypes.
     */
    explicit DrawablesGroup(NativeClient& client, Drawable::BaseContainer& common)
        : client(&client)
      , common(&common)
    {
    }

    DrawablesGroup(DrawablesGroup const&)            = delete;
    DrawablesGroup& operator=(DrawablesGroup const&) = delete;
    DrawablesGroup(DrawablesGroup&&)                 = delete;
    DrawablesGroup& operator=(DrawablesGroup&&)      = delete;

    ~DrawablesGroup() override = default;

    /**
     * \brief Spool a number of drawables. This fills the internal pool with new drawables.
     * @param count The number of drawables to spool.
     */
    void Spool(UINT const count) { for (UINT i = 0; i < count; i++) pool.push_back(std::make_unique<D>(*client)); }

    /**
     * \brief Creates and stores a new drawable.
     * \param initializer The initializer function.
     * \return A reference to the created drawable.
     */
    D& Create(std::function<void(D&)> initializer)
    {
        std::unique_ptr<D> stored;

        if (pool.empty()) stored = std::make_unique<D>(*client);
        else
        {
            stored = std::move(pool.back());
            pool.pop_back();
        }

        auto& object = *stored;

        Drawable::BaseIndex  base  = common->Push(&object);
        Drawable::EntryIndex entry = entries.Push(std::move(stored));
        object.AssociateWithIndices(base, entry);

        initializer(object);

        return object;
    }

    /**
     * \brief Mark a drawable as modified.
     * \param drawable The drawable to mark.
     */
    void MarkModified(D& drawable)
    {
        Drawable::EntryIndex const entry = drawable.GetEntryIndex();
        modified.Insert(entry);
    }

    /**
     * \brief Activate a drawable for rendering.
     * \param drawable The drawable to activate.
     */
    void Activate(D& drawable)
    {
        Require(!drawable.GetActiveIndex());

        Drawable::ActiveIndex activeIndex = active.Push(&drawable);
        activated.Insert(activeIndex);

        drawable.SetActiveIndex(activeIndex);
    }

    /**
     * \brief Deactivate a drawable.
     * \param drawable The drawable to deactivate.
     */
    void Deactivate(D& drawable)
    {
        Require(drawable.GetActiveIndex().has_value());

        Drawable::ActiveIndex const activeIndex = *drawable.GetActiveIndex();
        active.Pop(activeIndex);
        activated.Erase(activeIndex);

        drawable.SetActiveIndex(std::nullopt);
    }

    /**
     * \brief Return a drawable to the creator.
     * \param drawable The drawable to return.
     */
    void Return(D& drawable)
    {
        Drawable::EntryIndex const entry = drawable.GetEntryIndex();
        Drawable::BaseIndex const  base  = drawable.GetHandle();

        modified.Erase(entry);
        common->Pop(base);

        std::unique_ptr<D> object = entries.Pop(entry);
        object->Reset();
        pool.push_back(std::move(object));
    }

    /**
     * \brief Get the bag of active drawables.
     * \return The bag of active drawables.
     */
    Bag<D*, Drawable::ActiveIndex>& GetActive() { return active; }

    /**
     * \brief Get all modified drawables.
     * \return A range of all modified drawables.
     */
    auto GetModified()
    {
        return std::ranges::views::transform(
            modified,
            [this](Drawable::EntryIndex const entry) -> D*
            {
                Require(entries[entry] != nullptr);
                return entries[entry].get();
            });
    }

    [[nodiscard]] size_t GetModifiedCount() const { return modified.Count(); }

    /**
     * \brief Get all changed drawables. A drawable is changed if it is active and either newly activated or modified.
     * \return A range of all changed drawables.
     */
    IntegerSet<> ClearChanged()
    {
        IntegerSet<> changed(activated);
        for (Drawable::EntryIndex const entry : modified)
        {
            Require(entries[entry] != nullptr);
            auto activeIndex = entries[entry]->GetActiveIndex();

            if (activeIndex) changed.Insert(static_cast<size_t>(*activeIndex));
        }

        activated.Clear();

        return changed;
    }

    void EnqueueDataUpload(ComPtr<ID3D12GraphicsCommandList4> commandList) override
    {
        std::vector<D3D12_RESOURCE_BARRIER> barriers;
        barriers.reserve(modified.Count());

        for (Drawable::EntryIndex const entry : modified)
        {
            Require(entries[entry] != nullptr);
            entries[entry]->EnqueueDataUpload(commandList, &barriers);
        }

        if (!barriers.empty()) commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
    }

    void CleanupDataUpload() override
    {
        for (Drawable::EntryIndex const entry : modified)
        {
            Require(entries[entry] != nullptr);
            entries[entry]->CleanupDataUpload();
        }
        modified.Clear();
    }

private:
    NativeClient*            client;
    Drawable::BaseContainer* common;

    Bag<std::unique_ptr<D>, Drawable::EntryIndex> entries = {};
    std::vector<std::unique_ptr<D>>               pool    = {};

    IntegerSet<Drawable::EntryIndex>  modified  = {};
    IntegerSet<Drawable::ActiveIndex> activated = {};
    Bag<D*, Drawable::ActiveIndex>    active    = {};
};
