// <copyright file="Marshalling.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
        for (var index = 0; index < length; index++) TMarshaller.Free(unmanaged[index]);

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
    public static abstract TUnmanaged ConvertToUnmanaged(TManaged managed);

    /// <summary>
    ///     Free an unmanaged value.
    /// </summary>
    /// <param name="unmanaged">The unmanaged value to free, created by <see cref="ConvertToUnmanaged" />.</param>
    public static abstract void Free(TUnmanaged unmanaged);
}
