// <copyright file="ReadOnlySlot.cs" company="VoxelGame">
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
using System.Collections.Generic;

namespace VoxelGame.GUI.Bindings;

/// <summary>
///     A read-only slot stores a value that can be retrieved and subscribed to for changes, but not set externally.
/// </summary>
/// <typeparam name="T">The type of value stored in the slot.</typeparam>
public class ReadOnlySlot<T> : IValueSource<T>
{
    // todo: maybe remove ReadOnlySlot and just use IValueSource<T> everywhere, move code down to Slot<T>
    // todo: maybe the same can be done for the list slot as well

    private readonly Object source;

    private T value;

    /// <summary>
    ///     Creates a new instance of the <see cref="ReadOnlySlot{T}" /> class.
    /// </summary>
    /// <param name="value">The initial value of the slot.</param>
    /// <param name="source">The source for change events, ideally the owner of this class.</param>
    public ReadOnlySlot(T value, Object source)
    {
        this.value = value;
        this.source = source;
    }

    /// <inheritdoc />
    public T GetValue()
    {
        return value;
    }

    /// <inheritdoc />
    public event EventHandler? ValueChanged;

    /// <summary>
    ///     Set the value of the slot.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    protected void SetValue(T newValue)
    {
        T oldValue = value;

        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

        value = newValue;

        ValueChanged?.Invoke(source, EventArgs.Empty);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return $"{{{GetValue()?.ToString()}}}";
    }
}
