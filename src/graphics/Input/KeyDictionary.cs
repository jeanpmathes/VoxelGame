// <copyright file="KeyDictionary.cs" company="VoxelGame">
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     Utility to map <see cref="VirtualKeys" />s to values of an arbitrary type.
/// </summary>
/// <typeparam name="TValue">The arbitrary type.</typeparam>
internal class KeyDictionary<TValue> : IEnumerable<(VirtualKeys key, TValue value)> where TValue : notnull
{
    private readonly LinkedListNode<(VirtualKeys key, TValue value)>?[] references = new LinkedListNode<(VirtualKeys key, TValue value)>[0xFF];
    private readonly LinkedList<(VirtualKeys key, TValue value)> entries = [];

    static KeyDictionary()
    {
        Debug.Assert((Int32) VirtualKeys.LastKey < 0xFF);
    }

    /// <summary>
    ///     Adds the specified key and value pair to the dictionary. If the key already exists, its value is updated.
    /// </summary>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to be added or updated in the dictionary.</param>
    internal void Add(VirtualKeys key, TValue value)
    {
        LinkedListNode<(VirtualKeys key, TValue value)>? reference = references[(Int32) key];

        if (reference != null)
        {
            reference.Value = (key, value);
            return;
        }

        reference = entries.AddLast((key, value));
        references[(Int32) key] = reference;
    }

    /// <summary>
    ///     Attempts to retrieve the value associated with the specified key in the dictionary.
    /// </summary>
    /// <param name="key">The key for the value to locate in the dictionary.</param>
    /// <param name="value">
    ///     When this method returns, contains the value associated with the specified key, if the key was
    ///     found; otherwise, the default value for the type of the value parameter.
    /// </param>
    /// <returns>True if the dictionary contains the specified key; otherwise, false.</returns>
    internal Boolean TryGetValue(VirtualKeys key, [NotNullWhen(returnValue: true)] out TValue? value)
    {
        LinkedListNode<(VirtualKeys key, TValue value)>? reference = references[(Int32) key];

        if (reference == null)
        {
            value = default;
            return false;
        }

        value = reference.Value.value;
        return true;
    }

    /// <summary>
    ///     Removes the entry associated with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the entry to be removed.</param>
    /// <param name="value">
    ///     When the method returns, contains the value associated with the specified key if the key is found;
    ///     otherwise, the default value for the type of the value parameter.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the entry is successfully found and removed; otherwise, <see langword="false" />.
    /// </returns>
    internal Boolean Remove(VirtualKeys key, [NotNullWhen(returnValue: true)] out TValue? value)
    {
        LinkedListNode<(VirtualKeys key, TValue value)>? reference = references[(Int32) key];

        if (reference == null)
        {
            value = default;
            return false;
        }

        value = reference.Value.value;

        entries.Remove(reference);
        references[(Int32) key] = null;

        return true;
    }

    /// <summary>
    ///     Clears all entries in the dictionary.
    /// </summary>
    internal void Clear()
    {
        foreach ((VirtualKeys key, TValue _) in entries)
        {
            references[(Int32) key] = null;
        }

        entries.Clear();
    }

    #region ENUMERABLE

    /// <summary>
    ///     Get an enumerator for the dictionary.
    /// </summary>
    public LinkedList<(VirtualKeys key, TValue value)>.Enumerator GetEnumerator()
    {
        return entries.GetEnumerator();
    }

    IEnumerator<(VirtualKeys key, TValue value)> IEnumerable<(VirtualKeys key, TValue value)>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion ENUMERABLE
}
