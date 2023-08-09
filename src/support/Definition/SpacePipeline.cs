// <copyright file="SpacePipeline.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;

namespace VoxelGame.Support.Definition;

/// <summary>
///     Describes the raytracing pipeline that renders the 3D space.
/// </summary>
public sealed record SpacePipeline
{
    internal ShaderFileDescription[] ShaderFiles { get; init; } = null!;
    internal string[] Symbols { get; init; } = null!;
    internal MaterialDescription[] Materials { get; init; } = null!;
    internal IntPtr[] TexturePointers { get; init; } = null!;
    internal SpacePipelineDescription Description { get; init; }
}

/// <summary>
///     Additional information describing the raytracing pipeline.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
internal struct SpacePipelineDescription
#pragma warning restore S3898 // No equality comparison used.
{
    internal uint shaderCount;
    internal uint materialCount;

    internal uint textureCountFirstSlot;
    internal uint textureCountSecondSlot;

    internal Native.NativeErrorMessageFunc onShaderLoadingError;
}

/// <summary>
///     Describes a shader file that is loaded into the raytracing pipeline.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
internal struct ShaderFileDescription
#pragma warning restore S3898 // No equality comparison used.
{
    [MarshalAs(UnmanagedType.LPWStr)] internal string path;

    internal uint symbolCount;
}

/// <summary>
///     Describes a material that is loaded into the raytracing pipeline.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
internal struct MaterialDescription
#pragma warning restore S3898 // No equality comparison used.
{
    [MarshalAs(UnmanagedType.LPWStr)] internal string debugName;
    [MarshalAs(UnmanagedType.Bool)] internal bool isOpaque;

    [MarshalAs(UnmanagedType.LPWStr)] internal string normalClosestHitSymbol;
    [MarshalAs(UnmanagedType.LPWStr)] internal string normalAnyHitSymbol;

    [MarshalAs(UnmanagedType.LPWStr)] internal string shadowClosestHitSymbol;
    [MarshalAs(UnmanagedType.LPWStr)] internal string shadowAnyHitSymbol;
}
