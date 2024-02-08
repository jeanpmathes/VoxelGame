// <copyright file="IntegerSet.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Concepts.hpp"

class IntegerSetBase
{
protected:
    using BinaryData = uint64_t;

    static constexpr size_t BINARY_DATA_BITS = sizeof(BinaryData) * 8;
    static constexpr size_t BINARY_DATA_MASK = BINARY_DATA_BITS - 1;

    struct Data
    {
        size_t                  count = 0;
        std::vector<BinaryData> data  = {};
    };

    Data&                     data() { return m_content; }
    [[nodiscard]] Data const& data() const { return m_content; }

private:
    Data m_content = {};
};

/**
 * \brief A bit-based set of integers.
 * \tparam I The type of the integers to store.
 */
template <UnsignedNativeSizedInteger I = size_t>
class IntegerSet : IntegerSetBase
{
public:
    /** 
     * \brief Creates a set with the given number of elements, all set to true.
     * \param count The number of elements to create the set with.
     * \return The created set.
     */
    static IntegerSet Full(size_t count)
    {
        size_t const full      = count / BINARY_DATA_BITS;
        size_t const remainder = count & BINARY_DATA_MASK;

        size_t const required = full + (remainder > 0 ? 1 : 0);

        IntegerSet set;

        set.data().count = count;
        set.data().data.resize(required, static_cast<BinaryData>(-1));

        if (remainder > 0) set.data().data[full] = (static_cast<BinaryData>(1) << remainder) - 1;

        return set;
    }

    IntegerSet() = default;

    IntegerSet(IntegerSet const&)            = default;
    IntegerSet& operator=(IntegerSet const&) = default;
    IntegerSet(IntegerSet&&)                 = default;
    IntegerSet& operator=(IntegerSet&&)      = default;
    ~IntegerSet()                            = default;

    template <UnsignedNativeSizedInteger OtherI>
    friend class IntegerSet;

    template <UnsignedNativeSizedInteger OtherI>
    explicit IntegerSet(IntegerSet<OtherI> const& other) { *this = other; }

    template <UnsignedNativeSizedInteger OtherI>
    IntegerSet& operator=(IntegerSet<OtherI> const& other)
    {
        data().count = other.data().count;
        data().data  = other.data().data;

        return *this;
    }

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
        using difference_type = std::ptrdiff_t;
        using value_type = I;

        const_iterator() = default;
        const_iterator(
            std::vector<BinaryData>::const_iterator dataIterator, std::vector<BinaryData>::const_iterator dataEnd);
        const_iterator& operator++();
        const_iterator& operator++(int);
        bool            operator==(const_iterator const& other) const;
        bool            operator!=(const_iterator const& other) const;
        I               operator*() const;

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
};

template <UnsignedNativeSizedInteger I>
void IntegerSet<I>::Clear()
{
    data().count = 0;
    data().data.clear();
}

template <UnsignedNativeSizedInteger I>
void IntegerSet<I>::Insert(I element)
{
    size_t const index = static_cast<size_t>(element);

    size_t const dataIndex = index / BINARY_DATA_BITS;
    size_t const bitIndex  = index & BINARY_DATA_MASK;

    if (dataIndex >= data().data.size()) data().data.resize(dataIndex + 1, 0);

    size_t& content = data().data[dataIndex];

    if (!GetBit(content, bitIndex)) data().count += 1;

    content |= (static_cast<BinaryData>(1) << bitIndex);
}

template <UnsignedNativeSizedInteger I>
void IntegerSet<I>::Erase(I element)
{
    size_t const index = static_cast<size_t>(element);

    size_t const dataIndex = index / BINARY_DATA_BITS;
    size_t const bitIndex  = index & BINARY_DATA_MASK;

    if (dataIndex >= data().data.size()) return;

    size_t& content = data().data[dataIndex];

    if (GetBit(content, bitIndex)) data().count -= 1;

    content &= ~(static_cast<BinaryData>(1) << bitIndex);
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::Contains(I element) const
{
    size_t const index = static_cast<size_t>(element);

    size_t const dataIndex = index / BINARY_DATA_BITS;
    size_t const bitIndex  = index & BINARY_DATA_MASK;

    if (dataIndex >= data().data.size()) return false;

    return GetBit(data().data[dataIndex], bitIndex);
}

template <UnsignedNativeSizedInteger I>
size_t IntegerSet<I>::Count() const { return data().count; }

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::IsEmpty() const { return data().count == 0; }

template <UnsignedNativeSizedInteger I>
IntegerSet<I>::const_iterator::const_iterator(
    std::vector<BinaryData>::const_iterator const dataIterator, std::vector<BinaryData>::const_iterator const dataEnd)
    : m_dataIterator(dataIterator)
  , m_dataEnd(dataEnd)
  , m_inDataIndex(0)
  , m_totalIndex(0) { if (m_dataIterator != m_dataEnd && !GetBit(*m_dataIterator, m_inDataIndex)) Advance(); }

template <UnsignedNativeSizedInteger I>
typename IntegerSet<I>::const_iterator& IntegerSet<I>::const_iterator::operator++()
{
    Advance();
    return *this;
}

template <UnsignedNativeSizedInteger I>
typename IntegerSet<I>::const_iterator& IntegerSet<I>::const_iterator::operator++(int)
{
    auto copy = *this;
    Advance();
    return copy;
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::const_iterator::operator==(const_iterator const& other) const
{
    return std::tie(m_dataIterator, m_inDataIndex) == std::tie(other.m_dataIterator, other.m_inDataIndex);
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::const_iterator::operator!=(const_iterator const& other) const
{
    return std::tie(m_dataIterator, m_inDataIndex) != std::tie(other.m_dataIterator, other.m_inDataIndex);
}

template <UnsignedNativeSizedInteger I>
I IntegerSet<I>::const_iterator::operator*() const { return static_cast<I>(m_totalIndex); }

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
            std::next(m_dataIterator),
            0,
            m_totalIndex + (BINARY_DATA_BITS - m_inDataIndex));

    if (m_dataIterator == m_dataEnd) return;

    // Lastly, search for the next bit in the current data unit that is set.
    while (!GetBit(*m_dataIterator, m_inDataIndex))
        std::tie(m_inDataIndex, m_totalIndex) = std::make_tuple(m_inDataIndex + 1, m_totalIndex + 1);
}

template <UnsignedNativeSizedInteger I>
typename IntegerSet<I>::const_iterator IntegerSet<I>::begin() const
{
    return const_iterator(data().data.begin(), data().data.end());
}

template <UnsignedNativeSizedInteger I>
typename IntegerSet<I>::const_iterator IntegerSet<I>::end() const
{
    return const_iterator(data().data.end(), data().data.end());
}

template <UnsignedNativeSizedInteger I>
bool IntegerSet<I>::GetBit(BinaryData const data, size_t const bitIndex)
{
    return (data & (static_cast<BinaryData>(1) << bitIndex)) != 0;
}
