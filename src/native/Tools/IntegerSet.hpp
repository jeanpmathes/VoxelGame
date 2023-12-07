// <copyright file="IntegerSet.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

template <typename T>
concept UnsignedNativeSizedInteger = requires(T x, size_t y)
{
    static_cast<size_t>(x);
    static_cast<T>(y);
    sizeof(T) == sizeof(size_t);
};

/**
 * \brief A bit-based set of integers.
 * \tparam I The type of the integers to store.
 */
template <UnsignedNativeSizedInteger I = size_t>
class IntegerSet
{
public:
    using BinaryData = uint64_t;
    static constexpr size_t BINARY_DATA_BITS = sizeof(BinaryData) * 8;
    static constexpr size_t BINARY_DATA_MASK = BINARY_DATA_BITS - 1;

    /**
     * \brief Clears the set.
     */
    void Clear();

    /**
     * \brief Inserts the given element into the set.
     * \param element The element to insert.
     */
    void Insert(I element);

    /**
     * \brief Erases the given element from the set.
     * \param element The element to erase.
     */
    void Erase(I element);

    /**
     * \brief Checks if the set contains the given element.
     * \param element The element to check.
     * \return Whether the set contains the given element.
     */
    [[nodiscard]] bool Contains(I element) const;

    /**
     * \brief Gets the number of elements in the set.
     * \return The number of elements in the set.
     */
    [[nodiscard]] size_t Count() const;

    /**
     * \brief Checks if the set is empty.
     * \return True if the set is empty, false otherwise.
     */
    [[nodiscard]] bool IsEmpty() const;

    // ReSharper disable once CppInconsistentNaming
    class const_iterator
    {
    public:
        const_iterator(std::vector<BinaryData>::const_iterator dataIterator,
                       std::vector<BinaryData>::const_iterator dataEnd);
        const_iterator& operator++();
        bool operator!=(const const_iterator& other) const;
        I operator*() const;

    private:
        void Advance();

        std::vector<BinaryData>::const_iterator m_dataIterator;
        std::vector<BinaryData>::const_iterator m_dataEnd;

        size_t m_inDataIndex;
        size_t m_totalIndex;
    };

    const_iterator begin() const;
    const_iterator end() const;

private:
    static bool GetBit(BinaryData data, size_t bitIndex);

    size_t m_count = 0;
    std::vector<BinaryData> m_data = {};
};

template <UnsignedNativeSizedInteger I>
void IntegerSet<I>::Clear()
{
    m_count = 0;
    m_data.clear();
}

template <UnsignedNativeSizedInteger I>
void IntegerSet<I>::Insert(I element)
{
    const size_t index = static_cast<size_t>(element);

    const size_t dataIndex = index / BINARY_DATA_BITS;
    const size_t bitIndex = index & BINARY_DATA_MASK;

    if (dataIndex >= m_data.size())
        m_data.resize(dataIndex + 1, 0);

    size_t& data = m_data[dataIndex];

    if (!GetBit(data, bitIndex))
        m_count++;

    data |= (static_cast<BinaryData>(1) << bitIndex);
}

template <UnsignedNativeSizedInteger I>
void IntegerSet<I>::Erase(I element)
{
    const size_t index = static_cast<size_t>(element);

    const size_t dataIndex = index / BINARY_DATA_BITS;
    const size_t bitIndex = index & BINARY_DATA_MASK;

    if (dataIndex >= m_data.size())
        return;

    size_t& data = m_data[dataIndex];

    if (GetBit(data, bitIndex))
        m_count--;

    data &= ~(static_cast<BinaryData>(1) << bitIndex);
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::Contains(I element) const
{
    const size_t index = static_cast<size_t>(element);

    const size_t dataIndex = index / BINARY_DATA_BITS;
    const size_t bitIndex = index & BINARY_DATA_MASK;

    if (dataIndex >= m_data.size())
        return false;

    return GetBit(m_data[dataIndex], bitIndex);
}

template <UnsignedNativeSizedInteger I>
size_t IntegerSet<I>::Count() const
{
    return m_count;
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::IsEmpty() const
{
    return m_count == 0;
}

template <UnsignedNativeSizedInteger I>
IntegerSet<I>::const_iterator::const_iterator(
    const std::vector<BinaryData>::const_iterator dataIterator,
    const std::vector<BinaryData>::const_iterator dataEnd)
    : m_dataIterator(dataIterator)
      , m_dataEnd(dataEnd)
      , m_inDataIndex(0)
      , m_totalIndex(0)
{
    if (m_dataIterator != m_dataEnd && !GetBit(*m_dataIterator, m_inDataIndex))
    {
        Advance();
    }
}

template <UnsignedNativeSizedInteger I>
typename IntegerSet<I>::const_iterator& IntegerSet<I>::const_iterator::operator++()
{
    Advance();
    return *this;
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::const_iterator::operator!=(const const_iterator& other) const
{
    return std::tie(m_dataIterator, m_inDataIndex) != std::tie(other.m_dataIterator, other.m_inDataIndex);
}

template <UnsignedNativeSizedInteger I>
I IntegerSet<I>::const_iterator::operator*() const
{
    return static_cast<I>(m_totalIndex);
}

template <UnsignedNativeSizedInteger I>
void IntegerSet<I>::const_iterator::Advance()
{
    if (m_dataIterator == m_dataEnd) return;

    // First step is to increment the in-data index and handle out-of-bounds.
    std::tie(m_inDataIndex, m_totalIndex) = std::make_tuple(m_inDataIndex + 1, m_totalIndex + 1);
    if (m_inDataIndex == BINARY_DATA_BITS)
        std::tie(m_inDataIndex, m_dataIterator) = std::make_tuple(0, std::next(m_dataIterator));

    // Then search for the next data unit that has a bit set that is not read yet.
    while (m_dataIterator != m_dataEnd && *m_dataIterator >> m_inDataIndex == 0)
        std::tie(m_dataIterator, m_inDataIndex, m_totalIndex) = std::make_tuple(
            std::next(m_dataIterator), 0, m_totalIndex + (BINARY_DATA_BITS - m_inDataIndex));

    if (m_dataIterator == m_dataEnd) return;

    // Lastly, search for the next bit in the current data unit that is set.
    while (!GetBit(*m_dataIterator, m_inDataIndex))
        std::tie(m_inDataIndex, m_totalIndex) = std::make_tuple(m_inDataIndex + 1, m_totalIndex + 1);
}

template <UnsignedNativeSizedInteger I>
typename IntegerSet<I>::const_iterator IntegerSet<I>::begin() const
{
    return const_iterator(m_data.begin(), m_data.end());
}

template <UnsignedNativeSizedInteger I>
typename IntegerSet<I>::const_iterator IntegerSet<I>::end() const
{
    return const_iterator(m_data.end(), m_data.end());
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::GetBit(const BinaryData data, const size_t bitIndex)
{
    return (data & (static_cast<BinaryData>(1) << bitIndex)) != 0;
}
