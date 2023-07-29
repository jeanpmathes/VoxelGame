// <copyright file="SystemInformation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Definition;

/// <summary>
///     Contains general information about the (DirectX) system capabilities and limits.
/// </summary>
/// <param name="MaxTextureArrayAxisDimension">The maximum number of texture array elements in a texture array.</param>
public record struct SystemInformation(int MaxTextureArrayAxisDimension);
