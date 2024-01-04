// <copyright file="PipelineDescription.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;

namespace VoxelGame.Support.Definition;

#pragma warning disable S3898 // No equality comparison used.
#pragma warning disable S4022 // Enum storage type is explicit as it is passed to native code.

/// <summary>
///     Describes a pipeline for raster-based rendering.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RasterPipelineDescription
{
    /// <summary>
    ///     Path to the vertex shader.
    /// </summary>
    [MarshalAs(UnmanagedType.LPWStr)] private string VertexShaderPath;

    /// <summary>
    ///     Path to the pixel shader.
    /// </summary>
    [MarshalAs(UnmanagedType.LPWStr)] private string PixelShaderPath;

    /// <summary>
    ///     The shader preset.
    /// </summary>
    private ShaderPreset ShaderPreset;

    /// <summary>
    ///     The size of the shader constant buffer, or 0 if no constant buffer is used.
    /// </summary>
    internal uint BufferSize;

    /// <summary>
    ///     The topology of the mesh. Only used for <see cref="ShaderPreset.SpatialEffect" />.
    /// </summary>
    private Topology Topology;

    /// <summary>
    ///     Creates a new pipeline description.
    /// </summary>
    /// <param name="shader">The combined shader file.</param>
    /// <param name="preset">The shader preset.</param>
    /// <param name="topology">If the preset is <see cref="ShaderPreset.SpatialEffect"/>, the topology of the mesh.</param>
    /// <returns>The pipeline description.</returns>
    public static RasterPipelineDescription Create(FileInfo shader, ShaderPreset preset, Topology topology = Topology.Triangle)
    {
        return new RasterPipelineDescription
        {
            VertexShaderPath = shader.FullName,
            PixelShaderPath = shader.FullName,
            ShaderPreset = preset,
            BufferSize = 0,
            Topology = topology
        };
    }
}

/// <summary>
///     A shader preset determining the shader input and the root signature.
/// </summary>
public enum ShaderPreset : byte
{
    /// <summary>
    ///     Draws a single quad with a texture containing the previously rendered space.
    /// </summary>
    PostProcessing,

    /// <summary>
    ///     Used for drawing 2D rectangles that are either colored or textured.
    /// </summary>
    Draw2D,

    /// <summary>
    ///     Used for drawing 3D objects in the space, using a raster pipeline.
    /// </summary>
    SpatialEffect
}

/// <summary>
///     The topology of the raster pipeline. Only used for <see cref="ShaderPreset.SpatialEffect" />.
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
