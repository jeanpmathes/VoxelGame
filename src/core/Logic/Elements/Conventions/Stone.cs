// <copyright file="Stone.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors.Connection;
using VoxelGame.Core.Logic.Elements.Behaviors.Materials;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
/// A stone type, as defined by the <see cref="StoneConvention"/>.
/// </summary>
public class Stone(String namedID, BlockBuilder builder) : Convention<Stone>(namedID, builder)
{
    /// <summary>
    /// The base stone block of this stone type.
    /// </summary>
    public required Block Base { get; init; }
    
    /// <summary>
    /// When braking the base stone, it will break into rubble.
    /// The block is loose and as such allows water to flow through it.
    /// </summary>
    public required Block Rubble { get; init; }
    
    /// <summary>
    /// A worked stone block of this stone type, which is the result of processing the base stone.
    /// </summary>
    public required Block Worked { get; init; }
    
    /// <summary>
    /// A decorated stone block of this stone type, which is the result of decorating the worked stone.
    /// It shows a unique pattern on one of the sides.
    /// </summary>
    public required Block Decorated { get; init; }
    
    /// <summary>
    /// Pieces of this stone type, connected by mortar, to form basic road paving.
    /// The rough surface is not ideal for carts.
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
/// A convention on stone types.
/// </summary>
public static class StoneConvention // todo: check language for unused strings and remove them, most of the stone stuff would be unused
{
    /// <summary>
    /// Build a new stone type.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="name">The name of the stone, used for display purposes.</param>
    /// <param name="namedID">The named ID of the stone, used to create the block IDs.</param>
    /// <returns>The created stone type.</returns>
    public static Stone BuildStone(this BlockBuilder b, String name, String namedID)
    {
        return b.BuildConvention<Stone>(builder =>
        {
            String texture = namedID.PascalCaseToSnakeCase();

            return new Stone(namedID, builder)
            {
                Base = builder
                    .BuildSimpleBlock(name, $"{namedID}{nameof(Stone.Base)}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block(texture, x: 0)))
                    .Complete(),

                Rubble = builder
                    .BuildSimpleBlock($"{nameof(Language.Rubble)} ({name})", $"{namedID}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block(texture, x: 1)))
                    .WithBehavior<WetTint>()
                    .WithBehavior<Loose>()
                    .Complete(),

                Worked = builder
                    .BuildSimpleBlock($"{nameof(Language.WorkedStone)} ({name})", $"{namedID}{nameof(Stone.Worked)}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_worked", x: 0)))
                    .Complete(),

                Decorated = builder
                    .BuildSimpleBlock($"{nameof(Language.DecoratedStone)} ({name})", $"{namedID}{nameof(Stone.Decorated)}")
                    .WithTextureLayout(TextureLayout.UniqueFront(TID.Block($"{texture}_worked_decorated"), TID.Block($"{texture}_worked")))
                    .WithBehavior<LateralRotatable>()
                    // todo: probably needs a behavior for the placement logic that places with the correct orientation
                    // todo: could maybe be unified so it works for AxisRotatable as well
                    .Complete(),

                Cobblestone = builder
                    .BuildSimpleBlock($"{nameof(Language.Cobbles)} ({name})", $"{namedID}{nameof(Stone.Cobblestone)}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_cobbles", x: 0)))
                    .Complete(),

                Paving = builder
                    .BuildSimpleBlock($"{nameof(Language.Paving)} ({name})", $"{namedID}{nameof(Stone.Paving)}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_paving", x: 0)))
                    .Complete(),

                Bricks = builder
                    .BuildSimpleBlock($"{nameof(Language.Bricks)} ({name})", $"{namedID}{nameof(Stone.Bricks)}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_bricks", x: 0)))
                    .WithBehavior<ConstructionMaterial>()
                    .Complete(),

                Wall = builder
                    .BuildComplexBlock($"{nameof(Language.Wall)} ({name})", $"{namedID}{nameof(Stone.Wall)}")
                    .WithBehavior<Wall>()
                    .WithBehavior<WideConnecting>(connecting =>
                    {
                        connecting.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("wall_post"), RID.File<BlockModel>("wall_extension"), RID.File<BlockModel>("wall_extension_straight")));
                        connecting.TextureOverrideInitializer.ContributeConstant(TID.Block(texture, x: 1));
                    })
                    .Complete(),

                BrickWall = builder
                    .BuildComplexBlock($"{nameof(Language.BrickWall)} ({name})", $"{namedID}{nameof(Stone.BrickWall)}")
                    .WithBehavior<Wall>()
                    .WithBehavior<WideConnecting>(connecting =>
                    {
                        connecting.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("wall_post"), RID.File<BlockModel>("wall_extension"), RID.File<BlockModel>("wall_extension_straight")));
                        connecting.TextureOverrideInitializer.ContributeConstant(TID.Block($"{texture}_bricks", x: 0));
                    })
                    .Complete()
            };
        });
    }
}
