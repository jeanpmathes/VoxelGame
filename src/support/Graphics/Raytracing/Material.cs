// <copyright file="Material.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Graphics.Raytracing;

/// <summary>
///     A material that can be used in the raytracing pipeline.
///     Materials are created during the raytracing pipeline creation.
/// </summary>
/// <param name="Index">The index of the material.</param>
public record Material(UInt32 Index);
