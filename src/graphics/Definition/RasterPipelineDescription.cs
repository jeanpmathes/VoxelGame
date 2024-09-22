// <copyright file="PipelineDescription.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using VoxelGame.Graphics.Interop;

namespace VoxelGame.Graphics.Definition;

#pragma warning disable S3898 // No equality comparison used.
#pragma warning disable S4022 // Enum storage type is explicit as it is passed to native code.

/// <summary>
///     Describes a pipeline for raster-based rendering.
/// </summary>
[NativeMarshalling(typeof(RasterPipelineDescriptionMarshaller))]
public struct RasterPipelineDescription
{
    /// <summary>
    ///     Path to the vertex shader.
    /// </summary>
    internal String VertexShaderPath { get; private init; }

    /// <summary>
    ///     Path to the pixel shader.
    /// </summary>
    internal String PixelShaderPath { get; private init; }

    /// <summary>
    ///     The shader preset.
    /// </summary>
    internal ShaderPresets.ShaderPreset ShaderPreset { get; private init; }

    /// <summary>
    ///     The size of the shader constant buffer, or 0 if no constant buffer is used.
    /// </summary>
    internal UInt32 BufferSize { get; set; }

    /// <summary>
    ///     The topology of the mesh. Only used for <see cref="ShaderPresets.ShaderPreset.SpatialEffect" />.
    /// </summary>
    internal Topology Topology { get; private init; }

    /// <summary>
    ///     The filter set on the texture sampler. Only used for <see cref="ShaderPresets.ShaderPreset.PostProcessing" /> and
    ///     <see cref="ShaderPresets.ShaderPreset.Draw2D" />.
    /// </summary>
    internal Filter Filter { get; private init; }

    /// <summary>
    ///     Creates a new pipeline description.
    /// </summary>
    /// <param name="shader">The combined shader file.</param>
    /// <param name="preset">The shader preset.</param>
    /// <returns>The pipeline description.</returns>
    public static RasterPipelineDescription Create(FileInfo shader, ShaderPresets.IPreset preset)
    {
        return new RasterPipelineDescription
        {
            VertexShaderPath = shader.FullName,
            PixelShaderPath = shader.FullName,
            ShaderPreset = preset.Preset,
            BufferSize = 0,
            Topology = preset.Topology,
            Filter = preset.Filter
        };
    }
}

[CustomMarshaller(typeof(RasterPipelineDescription), MarshalMode.ManagedToUnmanagedIn, typeof(RasterPipelineDescriptionMarshaller))]
internal static class RasterPipelineDescriptionMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(RasterPipelineDescription managed)
    {
        return new Unmanaged
        {
            vertexShaderPath = UnicodeStringMarshaller.ConvertToUnmanaged(managed.VertexShaderPath),
            pixelShaderPath = UnicodeStringMarshaller.ConvertToUnmanaged(managed.PixelShaderPath),
            shaderPreset = managed.ShaderPreset,
            bufferSize = managed.BufferSize,
            topology = managed.Topology,
            filter = managed.Filter
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        UnicodeStringMarshaller.Free(unmanaged.vertexShaderPath);
        UnicodeStringMarshaller.Free(unmanaged.pixelShaderPath);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct Unmanaged
    {
        internal IntPtr vertexShaderPath;
        internal IntPtr pixelShaderPath;
        internal ShaderPresets.ShaderPreset shaderPreset;
        internal UInt32 bufferSize;
        internal Topology topology;
        internal Filter filter;
    }
}

/// <summary>
///     Utility class for shader presets.
/// </summary>
public static class ShaderPresets
{
    /// <summary>
    ///     All presets, as an enum for the native code.
    /// </summary>
    public enum ShaderPreset : byte
    {
        /// <summary>
        ///     The post processing preset, see <see cref="ShaderPresets.PostProcessing" />.
        /// </summary>
        PostProcessing,

        /// <summary>
        ///     The 2D drawing preset, see <see cref="ShaderPresets.Draw2D" />.
        /// </summary>
        Draw2D,

        /// <summary>
        ///     The 3D drawing preset, see <see cref="ShaderPresets.SpatialEffect" />.
        /// </summary>
        SpatialEffect
    }

    /// <summary>
    ///     Interface for shader presets.
    /// </summary>
    public interface IPreset
    {
        internal ShaderPreset Preset { get; }

        /// <summary>
        ///     Gets the topology of the mesh.
        /// </summary>
        public Topology Topology => Topology.Triangle;

        /// <summary>
        ///     Gets the filter set on the texture sampler.
        /// </summary>
        public Filter Filter => Filter.Linear;
    }

    /// <summary>
    ///     Draws a single quad with a texture containing the previously rendered space.
    /// </summary>
    public record struct PostProcessing(Filter Filter = Filter.Linear) : IPreset
    {
        /// <inheritdoc />
        public ShaderPreset Preset => ShaderPreset.PostProcessing;
    }

    /// <summary>
    ///     Used for drawing 2D rectangles that are either colored or textured.
    /// </summary>
    public record struct Draw2D(Filter Filter = Filter.Linear) : IPreset
    {
        /// <inheritdoc />
        public ShaderPreset Preset => ShaderPreset.Draw2D;
    }

    /// <summary>
    ///     Used for drawing 3D objects in the space, using a raster pipeline.
    /// </summary>
    public record struct SpatialEffect(Topology Topology = Topology.Triangle) : IPreset
    {
        /// <inheritdoc />
        public ShaderPreset Preset => ShaderPreset.SpatialEffect;
    }
}

/// <summary>
///     The topology of the raster pipeline.
/// </summary>
public enum Topology : byte
{
    /// <summary>
    ///     The mesh is a list of triangles.
    /// </summary>
    Triangle,

    /// <summary>
    ///     The mesh is a list of lines.
    /// </summary>
    Line
}

/// <summary>
///     The filter set on the texture sampler.
/// </summary>
public enum Filter : byte
{
    /// <summary>
    ///     A linear filter.
    /// </summary>
    Linear,

    /// <summary>
    ///     A nearest/point filter.
    /// </summary>
    Closest
}
