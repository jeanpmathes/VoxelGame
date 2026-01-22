// <copyright file="Marshalling.cs" company="VoxelGame">
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
using System.Runtime.InteropServices.Marshalling;

namespace VoxelGame.Toolkit.Interop;

/// <summary>
///     Helper class for marshalling data between managed and unmanaged code.
/// </summary>
public static class Marshalling
{
    /// <summary>
    ///     Convert a managed array to an unmanaged array.
    /// </summary>
    /// <param name="managed">The managed array to convert.</param>
    /// <param name="length">The length of the unmanaged array elements.</param>
    /// <typeparam name="TManaged">The type of the managed array.</typeparam>
    /// <typeparam name="TUnmanaged">The type of the unmanaged array elements.</typeparam>
    /// <typeparam name="TMarshaller">The marshaller for the managed and unmanaged types.</typeparam>
    /// <returns>The unmanaged array.</returns>
    public static unsafe TUnmanaged* ConvertToUnmanaged<TManaged, TUnmanaged, TMarshaller>(TManaged[] managed, out UInt32 length)
        where TUnmanaged : unmanaged
        where TMarshaller : IMarshaller<TManaged, TUnmanaged>
    {
        TUnmanaged* unmanaged = ArrayMarshaller<TManaged, TUnmanaged>
            .AllocateContainerForUnmanagedElements(managed, out Int32 num);

        ReadOnlySpan<TManaged> source = ArrayMarshaller<TManaged, TUnmanaged>
            .GetManagedValuesSource(managed);

        Span<TUnmanaged> destination = ArrayMarshaller<TManaged, TUnmanaged>
            .GetUnmanagedValuesDestination(unmanaged, num);

        for (var index = 0; index < num; index++) destination[index] = TMarshaller.ConvertToUnmanaged(source[index]);

        length = (UInt32) num;

        return unmanaged;
    }

    /// <summary>
    ///     Free an unmanaged array created by <see cref="ConvertToUnmanaged{TUnmanaged,TMarshaller,TManaged}" />.
    /// </summary>
    /// <param name="unmanaged">The unmanaged array to free.</param>
    /// <param name="length">The length of the unmanaged array.</param>
    /// <typeparam name="TManaged">The type of the managed array elements.</typeparam>
    /// <typeparam name="TUnmanaged">The type of the unmanaged array elements.</typeparam>
    /// <typeparam name="TMarshaller">The marshaller for the managed and unmanaged types.</typeparam>
    public static unsafe void Free<TManaged, TUnmanaged, TMarshaller>(TUnmanaged* unmanaged, UInt32 length)
        where TUnmanaged : unmanaged
        where TMarshaller : IMarshaller<TManaged, TUnmanaged>
    {
        for (UInt32 index = 0; index < length; index++) TMarshaller.Free(unmanaged[index]);

        ArrayMarshaller<TManaged, TUnmanaged>.Free(unmanaged);
    }
}

/// <summary>
///     Interface to access marshaller operations of custom marshallers for utility methods.
/// </summary>
public interface IMarshaller<in TManaged, TUnmanaged>
    where TUnmanaged : unmanaged
{
    /// <summary>
    ///     Convert a managed value to an unmanaged value.
    /// </summary>
    /// <param name="managed">The managed value to convert.</param>
    /// <returns>The unmanaged value.</returns>
    static abstract TUnmanaged ConvertToUnmanaged(TManaged managed);

    /// <summary>
    ///     Free an unmanaged value.
    /// </summary>
    /// <param name="unmanaged">The unmanaged value to free, created by <see cref="ConvertToUnmanaged" />.</param>
    static abstract void Free(TUnmanaged unmanaged);
}
