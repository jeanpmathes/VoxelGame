// <copyright file="FluidMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Data for meshing fluids.
/// </summary>
public sealed class FluidMeshData
{
    private FluidMeshData(int textureIndex, TintColor tint)
    {
        TextureIndex = textureIndex;
        Tint = tint;
    }

    /// <summary>
    ///     The texture index.
    /// </summary>
    public int TextureIndex { get; }

    /// <summary>
    ///     The tint color.
    /// </summary>
    public TintColor Tint { get; }

    /// <summary>
    ///     Creates fluid mesh data for an empty fluid.
    /// </summary>
    public static FluidMeshData Empty { get; } = new(textureIndex: 0, TintColor.None);

    /// <summary>
    ///     Creates fluid mesh data for a basic fluid.
    /// </summary>
    /// <param name="textureIndex">The texture index.</param>
    /// <param name="tint">The tint color.</param>
    /// <returns>The mesh data.</returns>
    public static FluidMeshData Basic(int textureIndex, TintColor tint)
    {
        return new FluidMeshData(textureIndex, tint);
    }
}
