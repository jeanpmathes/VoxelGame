// <copyright file="GappedList.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <vector>
#include <queue>

template <typename T>
concept Nullable = requires(T t)
{
    { t == nullptr } -> std::convertible_to<bool>;
    { t != nullptr } -> std::convertible_to<bool>;
};

/**
 * A list that can have gaps in it.
 * When pushing, it will try to fill the gaps first.
 */
template <Nullable E>
class GappedList
{
public:
    /**
     * Push an element to the list, filling a gap if possible.
     * This returns the index of the element, which can be used to remove it.
     */
    size_t Push(E element)
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
        return index;
    }

    /**
     * Remove an element from the list.
     */
    void Pop(size_t index)
    {
        REQUIRE(index < m_elements.size());
        REQUIRE(m_elements[index] != nullptr);

        m_elements[index] = nullptr;
        m_gaps.push(index);

        m_size--;
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

    E& operator[](size_t index)
    {
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
