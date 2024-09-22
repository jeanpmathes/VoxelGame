// <copyright file="GappedList.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <queue>
#include <vector>

#include "Concepts.hpp"

/**
 * A collection to store elements in. The collection allows pushing, popping and iterating over the elements. All elements in the collections are addressed by a unique index.
 */
template <Nullable E, UnsignedNativeSizedInteger I = size_t>
class Bag
{
public:
    /**
     * Push an element to the list, filling a gap if possible.
     * This returns the index of the element, which can be used to remove it.
     */
    I Push(E element)
    {
        Require(element != nullptr);

        size_t index;
        if (m_gaps.empty())
        {
            m_elements.emplace_back(std::move(element));
            index = m_elements.size() - 1;
        }
        else
        {
            index = m_gaps.top();
            m_gaps.pop();
            m_elements[index] = std::move(element);
        }

        m_size++;
        return static_cast<I>(index);
    }

    /**
     * Remove an element from the list.
     */
    E Pop(I i)
    {
        auto const index = static_cast<size_t>(i);

        Require(index < m_elements.size());
        Require(m_elements[index] != nullptr);

        auto element      = std::move(m_elements[index]);
        m_elements[index] = nullptr;

        m_gaps.push(index);
        m_size--;

        return element;
    }

    [[nodiscard]] size_t GetCount() const { return m_size; }

    [[nodiscard]] size_t GetCapacity() const { return m_elements.size(); }

    [[nodiscard]] bool IsEmpty() const { return m_size == 0; }

    E& operator[](I i)
    {
        auto const index = static_cast<size_t>(i);

        Require(index < m_elements.size());
        Require(m_elements[index] != nullptr);

        return m_elements[index];
    }

    /**
     * \brief Run a function on each element in the list.
     * \tparam F The type of the function to run.
     * \param f The function to run.
     */
    template <typename F>
    void ForEach(F f)
    {
        size_t done = 0;

        for (size_t index = 0; index < m_elements.size() && done < m_size; index++)
            if (m_elements[index] != nullptr)
            {
                f(m_elements[index]);
                done++;
            }
    }

private:
    std::vector<E>                                                         m_elements = {};
    std::priority_queue<size_t, std::vector<size_t>, std::greater<size_t>> m_gaps     = {};
    size_t                                                                 m_size     = 0;
};
