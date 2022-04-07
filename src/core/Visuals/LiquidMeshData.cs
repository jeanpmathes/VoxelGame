// <copyright file="LiquidMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Data for meshing liquids.
/// </summary>
public sealed class LiquidMeshData
{
    private LiquidMeshData(int textureIndex, TintColor tint)
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
    ///     Creates liquid mesh data for an empty liquid.
    /// </summary>
    public static LiquidMeshData Empty { get; } = new(textureIndex: 0, TintColor.None);

    /// <summary>
    ///     Creates liquid mesh data for a basic liquid.
    /// </summary>
    /// <param name="textureIndex">The texture index.</param>
    /// <param name="tint">The tint color.</param>
    /// <returns>The mesh data.</returns>
    public static LiquidMeshData Basic(int textureIndex, TintColor tint)
    {
        return new LiquidMeshData(textureIndex, tint);
    }
}
