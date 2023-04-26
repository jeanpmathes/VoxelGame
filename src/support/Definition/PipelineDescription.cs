// <copyright file="PipelineDescription.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;

namespace VoxelGame.Support.Definition;

/// <summary>
///     Describes a pipeline for raster-based rendering.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
public struct PipelineDescription
#pragma warning restore S3898 // No equality comparison used.
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
    public ulong BufferSize;

    /// <summary>
    ///     Creates a new pipeline description.
    /// </summary>
    /// <param name="shader">The combined shader file.</param>
    /// <param name="preset">The shader preset.</param>
    /// <returns>The pipeline description.</returns>
    public static PipelineDescription Create(FileInfo shader, ShaderPreset preset)
    {
        return new PipelineDescription
        {
            VertexShaderPath = shader.FullName,
            PixelShaderPath = shader.FullName,
            ShaderPreset = preset,
            BufferSize = 0
        };
    }
}

/// <summary>
///     A shader preset determining the shader input and the root signature.
/// </summary>
#pragma warning disable S4022 // Storage is explicit as it is passed to native code.
public enum ShaderPreset : byte
#pragma warning restore S4022 // Storage is explicit as it is passed to native code.
{
    /// <summary>
    ///     Draws a single quad with a texture containing the previously rendered space.
    /// </summary>
    PostProcessing,

    /// <summary>
    ///     Used for drawing 2D rectangles that are either colored or textured.
    /// </summary>
    Draw2D
}
