// <copyright file="GappedList.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <vector>
#include <queue>

#include "Concepts.hpp"

/**
 * A collection to store elements in.
 * The collection allows pushing, popping and iterating over the elements.
 * All elements in the collections are addressed by a unique index.
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
        REQUIRE(element != nullptr);

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
        const size_t index = static_cast<size_t>(i);
        
        REQUIRE(index < m_elements.size());
        REQUIRE(m_elements[index] != nullptr);

        auto element = std::move(m_elements[index]);
        m_elements[index] = nullptr;
        
        m_gaps.push(index);
        m_size--;

        return element;
    }

    [[nodiscard]] size_t GetCount() const
    {
        return m_size;
    }

    [[nodiscard]] size_t GetCapacity() const
    {
        return m_elements.size();
    }

    [[nodiscard]] bool IsEmpty() const
    {
        return m_size == 0;
    }

    E& operator[](I i)
    {
        const size_t index = static_cast<size_t>(i);
        
        REQUIRE(index < m_elements.size());
        REQUIRE(m_elements[index] != nullptr);

        return m_elements[index];
    }

    // ReSharper disable once CppInconsistentNaming
    class iterator
    {
    public:
        explicit iterator(typename std::vector<E>::iterator iterator, typename std::vector<E>::iterator end)
            : m_iterator(iterator), m_end(end)
        {
            if (m_iterator != m_end && *m_iterator == nullptr)
            {
                Advance();
            }
        }

        iterator operator++()
        {
            Advance();
            return *this;
        }

        bool operator!=(const iterator& other) const
        {
            return m_iterator != other.m_iterator;
        }

        E& operator*() const
        {
            return *m_iterator;
        }

    private:
        void Advance()
        {
            do
            {
                ++m_iterator;
            }
            while (m_iterator != m_end && *m_iterator == nullptr);
        }

        typename std::vector<E>::iterator m_iterator;
        typename std::vector<E>::iterator m_end;
    };

    iterator begin()
    {
        return iterator(m_elements.begin(), m_elements.end());
    }

    iterator end()
    {
        return iterator(m_elements.end(), m_elements.end());
    }

private:
    std::vector<E> m_elements = {};
    std::priority_queue<size_t, std::vector<size_t>, std::greater<size_t>> m_gaps = {};
    size_t m_size = 0;
};
