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
    [MarshalAs(UnmanagedType.LPWStr)] public string VertexShaderPath;

    /// <summary>
    ///     Path to the pixel shader.
    /// </summary>
    [MarshalAs(UnmanagedType.LPWStr)] public string PixelShaderPath;

    /// <summary>
    ///     The shader preset.
    /// </summary>
    public ShaderPreset ShaderPreset;
}

/// <summary>
///     A shader preset determining the shader input and the root signature.
/// </summary>
#pragma warning disable S4022 // Storage is explicit as it is passed to native code.
public enum ShaderPreset : byte
#pragma warning restore S4022 // Storage is explicit as it is passed to native code.
{
    /// <summary>
    ///     Draws in the 3D space.
    /// </summary>
    Space3D,

    /// <summary>
    ///     Draws a single quad with a texture containing the previously rendered space.
    /// </summary>
    PostProcessing
}

