// <copyright file="Fabricated.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Materials;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     These blocks are fabricated from other materials and are mostly decorative.
/// </summary>
public class Fabricated(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Wool is a flammable material, that allows its color to be changed.
    /// </summary>
    public Block Wool { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Wool)), Language.Wool)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("wool")))
        .WithBehavior<Combustible>()
        .WithBehavior<Paintable>()
        .Complete();

    /// <summary>
    ///     Decorated wool is similar to wool, decorated with golden ornaments.
    /// </summary>
    public Block WoolDecorated { get; } = builder
        .BuildSimpleBlock(new CID(nameof(WoolDecorated)), Language.WoolDecorated)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("wool_decorated")))
        .WithBehavior<Combustible>()
        .WithBehavior<Paintable>()
        .Complete();

    /// <summary>
    ///     Carpets can be used to cover the floor. Their color can be changed.
    /// </summary>
    public Block Carpet { get; } = builder
        .BuildComplexBlock(new CID(nameof(Carpet)), Language.Carpet)
        .WithBehavior<Modelled>(modelled => modelled.Layers.Initializer.ContributeConstant([RID.File<Model>("carpet")]))
        .WithBoundingVolume(new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)))
        .WithBehavior<Fillable>()
        .WithBehavior<Paintable>()
        .Complete();

    /// <summary>
    ///     Decorated carpets are similar to carpets, decorated with golden ornaments.
    /// </summary>
    public Block CarpetDecorated { get; } = builder
        .BuildComplexBlock(new CID(nameof(CarpetDecorated)), Language.CarpetDecorated)
        .WithBehavior<Modelled>(modelled => modelled.Layers.Initializer.ContributeConstant([RID.File<Model>("carpet_decorated")]))
        .WithBoundingVolume(new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)))
        .WithBehavior<Fillable>()
        .WithBehavior<Paintable>()
        .Complete();
}
