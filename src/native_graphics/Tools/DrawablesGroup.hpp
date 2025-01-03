﻿// <copyright file="DrawablesGroup.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
        : m_client(&client)
      , m_common(&common)
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
    void Spool(UINT const count) { for (UINT i = 0; i < count; i++) m_pool.push_back(std::make_unique<D>(*m_client)); }

    /**
     * \brief Creates and stores a new drawable.
     * \param initializer The initializer function.
     * \return A reference to the created drawable.
     */
    D& Create(std::function<void(D&)> initializer)
    {
        std::unique_ptr<D> stored;

        if (m_pool.empty()) stored = std::make_unique<D>(*m_client);
        else
        {
            stored = std::move(m_pool.back());
            m_pool.pop_back();
        }

        auto& object = *stored;

        Drawable::BaseIndex  base  = m_common->Push(&object);
        Drawable::EntryIndex entry = m_entries.Push(std::move(stored));
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
        m_modified.Insert(entry);
    }

    /**
     * \brief Activate a drawable for rendering.
     * \param drawable The drawable to activate.
     */
    void Activate(D& drawable)
    {
        Require(!drawable.GetActiveIndex());

        Drawable::ActiveIndex active = m_active.Push(&drawable);
        m_activated.Insert(active);

        drawable.SetActiveIndex(active);
    }

    /**
     * \brief Deactivate a drawable.
     * \param drawable The drawable to deactivate.
     */
    void Deactivate(D& drawable)
    {
        Require(drawable.GetActiveIndex().has_value());

        Drawable::ActiveIndex const active = *drawable.GetActiveIndex();
        m_active.Pop(active);
        m_activated.Erase(active);

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

        m_modified.Erase(entry);
        m_common->Pop(base);

        std::unique_ptr<D> object = m_entries.Pop(entry);
        object->Reset();
        m_pool.push_back(std::move(object));
    }

    /**
     * \brief Get the bag of active drawables.
     * \return The bag of active drawables.
     */
    Bag<D*, Drawable::ActiveIndex>& GetActive() { return m_active; }

    /**
     * \brief Get all modified drawables.
     * \return A range of all modified drawables.
     */
    auto GetModified()
    {
        return std::ranges::views::transform(
            m_modified,
            [this](Drawable::EntryIndex const entry) -> D* {
                Require(m_entries[entry] != nullptr);
                return m_entries[entry].get();
            });
    }

    [[nodiscard]] size_t GetModifiedCount() const { return m_modified.Count(); }

    /**
     * \brief Get all changed drawables. A drawable is changed if it is active and either newly activated or modified.
     * \return A range of all changed drawables.
     */
    IntegerSet<> ClearChanged()
    {
        IntegerSet<> changed(m_activated);
        for (Drawable::EntryIndex const entry : m_modified)
        {
            Require(m_entries[entry] != nullptr);
            auto active = m_entries[entry]->GetActiveIndex();

            if (active) changed.Insert(static_cast<size_t>(*active));
        }

        m_activated.Clear();

        return changed;
    }

    void EnqueueDataUpload(ComPtr<ID3D12GraphicsCommandList4> commandList) override
    {
        std::vector<D3D12_RESOURCE_BARRIER> barriers;
        barriers.reserve(m_modified.Count());

        for (Drawable::EntryIndex const entry : m_modified)
        {
            Require(m_entries[entry] != nullptr);
            m_entries[entry]->EnqueueDataUpload(commandList, &barriers);
        }

        if (!barriers.empty()) commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
    }

    void CleanupDataUpload() override
    {
        for (Drawable::EntryIndex const entry : m_modified)
        {
            Require(m_entries[entry] != nullptr);
            m_entries[entry]->CleanupDataUpload();
        }
        m_modified.Clear();
    }

private:
    NativeClient*            m_client;
    Drawable::BaseContainer* m_common;

    Bag<std::unique_ptr<D>, Drawable::EntryIndex> m_entries = {};
    std::vector<std::unique_ptr<D>>               m_pool    = {};

    IntegerSet<Drawable::EntryIndex>  m_modified  = {};
    IntegerSet<Drawable::ActiveIndex> m_activated = {};
    Bag<D*, Drawable::ActiveIndex>    m_active    = {};
};
