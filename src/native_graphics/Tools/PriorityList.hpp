// <copyright file="Common.hpp" company="VoxelGame">
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

/**
 * \brief A list of objects, ordered by priority.
 * 
 * This list can be used to implement layering of drawn things, which means that a higher priority comes later in the list.
 * This corresponds to drawing that thing later or on top.
 */
template <typename T>
class PriorityList
{
public:
    /**
     * Add an object to this priority list.
     * @param object The object. The list will take ownership.
     * @param priority The priority of the object. Use \c INT_MIN and \c INT_MAX to insert to the current front or back of the list.
     */
    void Add(std::unique_ptr<T> object, INT priority);

    /**
     * Remove an object from this priority list.
     * @param object The object to remove.
     */
    void Remove(T* object);

    /**
     * Check whether this list is empty.
     * \return \c true if it is empty, \c false otherwise. 
     */
    bool IsEmpty() const;

private:
    struct Entry
    {
        std::unique_ptr<T> object;
        INT                priority;
    };

    std::list<Entry>                                   entries;
    std::map<T*, typename decltype(entries)::iterator> entryMap = {};

public:
    // ReSharper disable once CppInconsistentNaming
    class iterator
    {
    public:
        using difference_type = std::ptrdiff_t;
        using value_type      = T;
        // ReSharper disable once CppInconsistentNaming
        using reference = T&;

        iterator() = default;
        explicit  iterator(decltype(entries)::iterator dataIterator);
        iterator& operator++();
        iterator& operator++(int);
        bool      operator==(iterator const& other) const;
        reference operator*() const;

    private:
        void Advance();

        decltype(entries)::iterator dataIterator;

        size_t inDataIndex = 0;
        size_t totalIndex  = 0;
    };

    iterator begin() { return iterator(entries.begin()); }
    iterator end() { return iterator(entries.end()); }
};

template <typename T>
void PriorityList<T>::Add(std::unique_ptr<T> object, INT priority)
{
    // INT_MIN and INT_MAX should always place the object at the front and back of the list, respectively.
    // Thus, all entries in the list should be in the range (INT_MIN, INT_MAX) - both exclusive.
    auto clampedPriority = static_cast<UINT>(std::clamp(priority, INT_MIN + 1, INT_MAX - 1));

    typename decltype(entries)::iterator iterator;
    T*                                   ptr = object.get();

    if (entries.empty() || priority < entries.front().priority)
    {
        entries.emplace_front(std::move(object), clampedPriority);
        iterator = entries.begin();
    }
    else if (priority > entries.back().priority)
    {
        entries.emplace_back(std::move(object), clampedPriority);
        iterator = std::prev(entries.end());
    }
    else
        for (auto it = entries.begin(); it != entries.end(); ++it)
            // Goal: insert after the first element with priority lower than the new one.
            if (priority > it->priority)
            {
                iterator = entries.emplace(std::prev(it), std::move(object), clampedPriority);
                break;
            }

    entryMap[ptr] = iterator;
}

template <typename T>
void PriorityList<T>::Remove(T* object)
{
    auto const iterator = entryMap.find(object);
    Require(iterator != entryMap.end());

    entries.erase(iterator->second);
}

template <typename T>
bool PriorityList<T>::IsEmpty() const
{
    return entries.empty();
}

template <typename T>
PriorityList<T>::iterator::iterator(typename decltype(entries)::iterator dataIterator)
    : dataIterator(dataIterator)
{
}

template <typename T>
PriorityList<T>::iterator& PriorityList<T>::iterator::operator++()
{
    Advance();
    return *this;
}

template <typename T>
PriorityList<T>::iterator& PriorityList<T>::iterator::operator++(int)
{
    auto copy = *this;
    Advance();
    return copy;
}

template <typename T>
bool PriorityList<T>::iterator::operator==(iterator const& other) const
{
    return dataIterator == other.dataIterator;
}

template <typename T>
PriorityList<T>::iterator::reference PriorityList<T>::iterator::operator*() const
{
    return *dataIterator->object;
}

template <typename T>
void PriorityList<T>::iterator::Advance()
{
    std::advance(dataIterator, 1);
}
