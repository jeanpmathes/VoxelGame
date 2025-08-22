// <copyright file="Palette.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Palettes;

/// <summary>
///     A palette of blocks and fluids to use for world generation.
/// </summary>
public sealed class Palette : IResource
{
    private readonly Content granite = new(Blocks.Instance.Stones.Granite.Base);
    private readonly Content limestone = new(Blocks.Instance.Stones.Limestone.Base);
    private readonly Content marble = new(Blocks.Instance.Stones.Marble.Base);
    private readonly Content sandstone = new(Blocks.Instance.Stones.Sandstone.Base);

    private readonly Content sand = new(Blocks.Instance.Environment.Sand);
    private readonly Content sandGroundwater = new(Blocks.Instance.Environment.Sand, Fluids.Instance.FreshWater);

    private readonly Content gravel = new(Blocks.Instance.Environment.Gravel);
    private readonly Content gravelGroundwater = new(Blocks.Instance.Environment.Gravel, Fluids.Instance.FreshWater);

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<Palette>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.GeneratorPalette;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

    internal Content GetStone(Map.StoneType type)
    {
        return type switch
        {
            Map.StoneType.Sandstone => sandstone,
            Map.StoneType.Granite => granite,
            Map.StoneType.Limestone => limestone,
            Map.StoneType.Marble => marble,
            _ => throw Exceptions.UnsupportedEnumValue(type)
        };
    }

    internal Content GetLoose(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sand : gravel;
    }

    internal Content GetGroundwater(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sandGroundwater : gravelGroundwater;
    }
}
