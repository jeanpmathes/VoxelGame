// <copyright file="RasterPipeline.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A pipeline for raster-based rendering.
/// </summary>
[NativeMarshalling(typeof(RasterPipelineMarshaller))]
public class RasterPipeline : NativeObject
{
    /// <summary>
    ///     Creates a new <see cref="RasterPipeline" />.
    /// </summary>
    public RasterPipeline(IntPtr nativePointer, Client client) : base(nativePointer, client) {}
}

#pragma warning disable S3242
[CustomMarshaller(typeof(RasterPipeline), MarshalMode.ManagedToUnmanagedIn, typeof(RasterPipelineMarshaller))]
internal static class RasterPipelineMarshaller
{
    internal static IntPtr ConvertToUnmanaged(RasterPipeline managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
