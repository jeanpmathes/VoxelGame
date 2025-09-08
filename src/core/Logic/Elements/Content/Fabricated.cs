// <copyright file="Fabricated.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Logic.Elements.Behaviors.Materials;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// These blocks are fabricated from other materials and are mostly decorative.
/// </summary>
public class Fabricated(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Wool is a flammable material, that allows its color to be changed.
    /// </summary>
    public Block Wool { get; } = builder
        .BuildSimpleBlock(nameof(Wool), Language.Wool)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("wool")))
        .WithBehavior<Combustible>()
        .WithBehavior<Paintable>()
        .Complete();
    
    /// <summary>
    ///     Decorated wool is similar to wool, decorated with golden ornaments.
    /// </summary>
    public Block WoolDecorated { get; } = builder
        .BuildSimpleBlock(nameof(WoolDecorated), Language.WoolDecorated)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("wool_decorated")))
        .WithBehavior<Combustible>()
        .WithBehavior<Paintable>()
        .Complete();
    
    /// <summary>
    ///     Carpets can be used to cover the floor. Their color can be changed.
    /// </summary>
    public Block Carpet { get; } = builder
        .BuildComplexBlock(nameof(Carpet), Language.Carpet)
        .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("carpet")]))
        .WithBoundingVolume(new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)))
        .WithBehavior<Fillable>()
        .WithBehavior<Paintable>()
        .Complete();
    
    /// <summary>
    ///     Decorated carpets are similar to carpets, decorated with golden ornaments.
    /// </summary>
    public Block CarpetDecorated { get; } = builder
        .BuildComplexBlock(nameof(CarpetDecorated), Language.CarpetDecorated)
        .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("carpet_decorated")]))
        .WithBoundingVolume(new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)))
        .WithBehavior<Fillable>()
        .WithBehavior<Paintable>()
        .Complete();
}
