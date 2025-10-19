// <copyright file="Stone.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Connection;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Materials;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Logic.Voxels.Behaviors.Siding;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A stone type, as defined by the <see cref="StoneConvention" />.
/// </summary>
public class Stone(CID contentID, BlockBuilder builder) : Convention<Stone>(contentID, builder)
{
    /// <summary>
    ///     The base stone block of this stone type.
    /// </summary>
    public required Block Base { get; init; }

    /// <summary>
        ///     When breaking the base stone, it will break into rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public required Block Rubble { get; init; }

    /// <summary>
    ///     A worked stone block of this stone type, which is the result of processing the base stone.
    /// </summary>
    public required Block Worked { get; init; }

    /// <summary>
    ///     A decorated stone block of this stone type, which is the result of decorating the worked stone.
    ///     It shows a unique pattern on one of the sides.
    /// </summary>
    public required Block Decorated { get; init; }

    /// <summary>
    ///     Pieces of this stone type, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public required Block Cobblestone { get; init; }

    /// <summary>
    ///     Paving made out of processed stone.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public required Block Paving { get; init; }

    /// <summary>
    ///     This stone type, cut into bricks and connected with mortar.
    /// </summary>
    public required Block Bricks { get; init; }

    /// <summary>
    ///     A wall made out of rubble of this stone type.
    /// </summary>
    public required Block Wall { get; init; }

    /// <summary>
    ///     A wall made out of bricks of this stone type.
    /// </summary>
    public required Block BrickWall { get; init; }
}

/// <summary>
///     A convention on stone types.
/// </summary>
public static class StoneConvention
{
    /// <summary>
    ///     Build a new stone type.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the stone, used to create the block CIDs.</param>
    /// <param name="name">The name of the stone, used for display purposes.</param>
    /// <returns>The created stone type.</returns>
    public static Stone BuildStone(this BlockBuilder b, CID contentID, String name)
    {
        return b.BuildConvention<Stone>(builder =>
        {
            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            return new Stone(contentID, builder)
            {
                Base = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Stone.Base)}"), name)
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block(texture, x: 0)))
                    .Complete(),

                Rubble = builder
                    .BuildSimpleBlock(new CID($"{contentID}"), $"{nameof(Language.Rubble)} ({name})")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block(texture, x: 1)))
                    .WithBehavior<WetTint>()
                    .WithBehavior<Loose>()
                    .Complete(),

                Worked = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Stone.Worked)}"), $"{Language.WorkedStone} ({name})")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_worked", x: 0)))
                    .Complete(),

                Decorated = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Stone.Decorated)}"), $"{Language.DecoratedStone} ({name})")
                    .WithTextureLayout(TextureLayout.UniqueFront(TID.Block($"{texture}_worked_decorated"), TID.Block($"{texture}_worked")))
                    .WithBehavior<LateralRotatable>()
                    .WithBehavior<DirectionalSidePlacement>()
                    .Complete(),

                Cobblestone = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Stone.Cobblestone)}"), $"{Language.Cobbles} ({name})")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_cobbles", x: 0)))
                    .Complete(),

                Paving = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Stone.Paving)}"), $"{Language.Paving} ({name})")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_paving", x: 0)))
                    .Complete(),

                Bricks = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Stone.Bricks)}"), $"{Language.Bricks} ({name})")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_bricks", x: 0)))
                    .WithBehavior<ConstructionMaterial>()
                    .Complete(),

                Wall = builder
                    .BuildComplexBlock(new CID($"{contentID}{nameof(Stone.Wall)}"), $"{Language.Wall} ({name})")
                    .WithBehavior<Wall>()
                    .WithBehavior<WideConnecting>(connecting => connecting.Models.Initializer.ContributeConstant((RID.File<Model>("wall_post"), RID.File<Model>("wall_extension"), RID.File<Model>("wall_extension_straight"))))
                    .WithTextureOverride(TextureOverride.All(TID.Block(texture, x: 1)))
                    .Complete(),

                BrickWall = builder
                    .BuildComplexBlock(new CID($"{contentID}{nameof(Stone.BrickWall)}"), $"{Language.BrickWall} ({name})")
                    .WithBehavior<Wall>()
                    .WithBehavior<WideConnecting>(connecting => connecting.Models.Initializer.ContributeConstant((RID.File<Model>("wall_post"), RID.File<Model>("wall_extension"), RID.File<Model>("wall_extension_straight"))))
                    .WithTextureOverride(TextureOverride.All(TID.Block($"{texture}_bricks", x: 0)))
                    .Complete()
            };
        });
    }
}
