// <copyright file="Construction.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Connection;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Logic.Elements.Behaviors.Height;
using VoxelGame.Core.Logic.Elements.Behaviors.Materials;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// Construction blocks are used by characters to build structures and walls.
/// They are generally not found naturally in the world, but are constructed intentionally.
/// </summary>
public class Construction(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Glass is a transparent block.
    /// </summary>
    public Block Glass { get; } = builder
        .BuildSimpleBlock(Language.Glass, nameof(Glass))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("glass")))
        .WithBehavior<Glass>()
        .Complete();
    
    /// <summary>
    ///     Tiled glass is like glass, but made out of four tiles.
    /// </summary>
    public Block GlassTiled { get; } = builder
        .BuildSimpleBlock(Language.TiledGlass, nameof(GlassTiled))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("glass_tiled")))
        .WithBehavior<Glass>()
        .Complete();
    
    /// <summary>
    ///     Small tiles for construction of floors and walls.
    /// </summary>
    public Block TilesSmall { get; } = builder
        .BuildSimpleBlock(Language.SmallTiles, nameof(TilesSmall))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("small_tiles")))
        .WithBehavior<ConstructionMaterial>()
        .Complete();
    
    /// <summary>
    ///     Large tiles for construction of floors and walls.
    /// </summary>
    public Block TilesLarge { get; } = builder
        .BuildSimpleBlock(Language.LargeTiles, nameof(TilesLarge))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("large_tiles")))
        .WithBehavior<ConstructionMaterial>()
        .Complete();
    
    /// <summary>
    ///     Clay bricks, placed as a block and connected with mortar.
    ///     This block is a construction material.
    /// </summary>
    public Block ClayBricks { get; } = builder
        .BuildSimpleBlock(Language.ClayBricks, nameof(ClayBricks))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("clay_bricks")))
        .WithBehavior<ConstructionMaterial>()
        .Complete();
    
    /// <summary>
    ///     Red plastic is a construction material.
    /// </summary>
    public Block RedPlastic { get; } = builder
        .BuildSimpleBlock(Language.RedPlastic, nameof(RedPlastic))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("red_plastic")))
        .WithBehavior<ConstructionMaterial>()
        .Complete();
    
    /// <summary>
    ///     Black checkerboard tiles come in different colors.
    /// </summary>
    public Block TilesCheckerboardBlack { get; } = builder
        .BuildSimpleBlock(Language.CheckerboardTilesBlack, nameof(TilesCheckerboardBlack))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("checkerboard_tiles_black")))
        .WithBehavior<ConstructionMaterial>()
        .WithBehavior<Paintable>()
        .Complete();
    
    /// <summary>
    ///     White checkerboard tiles come in different colors.
    /// </summary>
    public Block TilesCheckerboardWhite { get; } = builder
        .BuildSimpleBlock(Language.CheckerboardTilesWhite, nameof(TilesCheckerboardWhite))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("checkerboard_tiles_white")))
        .WithBehavior<ConstructionMaterial>()
        .WithBehavior<Paintable>()
        .Complete();
    
    /// <summary>
    ///     Concrete is a versatile construction material that can have different heights and colors.
    ///     It can be build using fluid concrete.
    /// </summary>
    public Block Concrete { get; } = builder // todo: check why debug view shows hight on placement as zero
        .BuildSimpleBlock(Language.Concrete, nameof(Concrete))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("concrete")))
        .WithBehavior<StoredHeight8>(height => height.PlacementHeightInitializer.ContributeConstant(StoredHeight8.MaximumHeight))
        .WithBehavior<Paintable>()
        .WithBehavior<PartialHeight, Connectable>((height, connectable) => connectable.IsConnectionAllowed.ContributeFunction((_, state) => height.IsFull(state)))
        .Complete();
    
    /// <summary>
    ///     A ladder allows climbing up and down.
    /// </summary>
    public Block Ladder { get; } = builder
        .BuildComplexBlock(Language.Ladder, nameof(Ladder))
        .WithBehavior<FlatModel>()
        .WithBehavior<SingleTextured>(texture => texture.DefaultTextureInitializer.ContributeConstant(TID.Block("ladder")))
        .WithBehavior<Climbable>(climbable => climbable.ClimbingVelocityInitializer.ContributeConstant(value: 3.0))
        .WithBehavior<FourWayRotatable>()
        .WithBehavior<Attached, SingleSided>((attached, siding) =>
        { 
            attached.AttachmentSidesInitializer.ContributeConstant(Sides.Lateral);
            
            attached.AttachedSides.ContributeFunction((_, state) => siding.GetSide(state).Opposite().ToFlag());
            attached.AttachedState.ContributeFunction((_, context) => siding.SetSide(context.state, context.sides.Single().Opposite())); // todo: handling if not single as this allows null, maybe a new extension for sides
        }) 
        .WithProperties(properties => properties.IsOpaque.ContributeConstant(value: false))
        .WithProperties(properties => properties.IsSolid.ContributeConstant(value: false))
        .Complete();
    
    /// <summary>
    ///     The vase is a decorative block that must be placed on solid ground.
    /// </summary>
    public Block Vase { get; } = builder
        .BuildComplexBlock(Language.Vase, nameof(Vase))
        .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("vase")]))
        .WithBoundingVolume(new BoundingVolume(new Vector3d(x: 0.5f, y: 0.375f, z: 0.5f), new Vector3d(x: 0.25f, y: 0.375f, z: 0.25f)))
        .WithBehavior<Fillable>()
        .WithBehavior<Grounded>()
        .Complete();
    
    /// <summary>
    ///     Glass panes are a thin alternative to glass blocks.
    ///     They connect to some neighboring blocks.
    /// </summary>
    public Block GlassPane { get; } = builder
        .BuildComplexBlock(Language.GlassPane, nameof(GlassPane))
        .WithBehavior<Glass>()
        .WithBehavior<ThinConnecting>(connecting => connecting.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("pane_glass_post"), RID.File<BlockModel>("pane_glass_side"), RID.File<BlockModel>("pane_glass_extension"))))
        .Complete();
    
    /// <summary>
    ///     Steel bars are a thin, but strong barrier.
    /// </summary>
    public Block Bars { get; } = builder
        .BuildComplexBlock(Language.Bars, nameof(Bars))
        .WithBehavior<ThinConnecting>(connecting => connecting.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("bars_post"), RID.File<BlockModel>("bars_side"), RID.File<BlockModel>("bars_extension"))))
        .Complete();
    
    /// <summary>
    ///     A wall constructed using clay bricks.
    ///     The wall does not prevent jumping over it, and can connect to other blocks.
    /// </summary>
    public Block ClayBrickWall { get; } = builder
        .BuildComplexBlock(Language.ClayBrickWall, nameof(ClayBrickWall))
        .WithBehavior<WideConnecting>(connecting =>
        {
            connecting.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("wall_post"), RID.File<BlockModel>("wall_extension"), RID.File<BlockModel>("wall_extension_straight")));
            connecting.TextureOverrideInitializer.ContributeConstant(TID.Block("clay_bricks"));
        })
        .WithBehavior<Wall>()
        .Complete();
    
    /// <summary>
    ///     The steel door allows closing of a room. It can be opened and closed.
    /// </summary>
    public Block DoorSteel { get; } = builder
        .BuildComplexBlock(Language.SteelDoor, nameof(DoorSteel))
        .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("door_steel_closed"), RID.File<BlockModel>("door_steel_open")]))
        .WithBehavior<Door>() 
        .Complete();
    
    /// <summary>
    ///     The fluid barrier can be used to control fluid flow. It can be opened and closed.
    ///     It does not prevent gasses from flowing through it.
    /// </summary>
    public Block FluidBarrier { get; } = builder
        .BuildSimpleBlock(Language.Barrier, nameof(FluidBarrier))
        .WithBehavior<CubeTextured, Barrier>((texture, barrier) => texture.ActiveTexture.ContributeFunction((_, state) => TextureLayout.Uniform(TID.Block("fluid_barrier", (Byte) (barrier.IsBarrierOpen(state) ? 0 : 1)))))
        .WithBehavior<Combustible>()
        .Complete();
    
    /// <summary>
    ///     The industrial steel pipe can be used to control fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public Block SteelPipe { get; } = builder
        .BuildComplexBlock(Language.SteelPipe, nameof(SteelPipe))
        .WithBehavior<Piped>(piped => piped.TierInitializer.ContributeConstant(Piped.PipeTier.Industrial))
        .WithBehavior<ConnectingPipe>(pipe => pipe.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("steel_pipe_center"), RID.File<BlockModel>("steel_pipe_connector"), RID.File<BlockModel>("steel_pipe_surface"))))
        .Complete();
    
    /// <summary>
    ///     This pipe is a special steel pipe that can only form straight connections.
    ///     It is ideal for parallel pipes.
    /// </summary>
    public Block StraightSteelPipe { get; } = builder
        .BuildComplexBlock(Language.SteelPipeStraight, nameof(StraightSteelPipe))
        .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("steel_pipe_straight")]))
        .WithBehavior<Piped>(piped => piped.TierInitializer.ContributeConstant(Piped.PipeTier.Industrial))
        .WithBehavior<StraightPipe>()
        .Complete();
    
    /// <summary>
    ///     This is a special steel pipe that can be closed. It prevents all fluid flow.
    /// </summary>
    public Block PipeValve { get; } = builder
        .BuildComplexBlock(Language.ValvePipe, nameof(PipeValve))
        .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("steel_pipe_valve_open"), RID.File<BlockModel>("steel_pipe_valve_closed")]))
        .WithBehavior<Piped>(piped => piped.TierInitializer.ContributeConstant(Piped.PipeTier.Industrial))
        .WithBehavior<StraightPipe>()
        .WithBehavior<Valve>()
        .Complete();
    
    /// <summary>
    ///     The pump can lift fluids up when interacted with.
    ///     It can only lift to a threshold of 16 blocks.
    /// </summary>
    public Block Pump { get; } = builder
        .BuildSimpleBlock(Language.Pump, nameof(Pump))
        .WithBehavior<CubeTextured>(texture => texture.DefaultTextureInitializer.ContributeConstant(TextureLayout.Uniform(TID.Block("pump"))))
        .WithBehavior<Piped>(piped => piped.TierInitializer.ContributeConstant(Piped.PipeTier.Industrial))
        .WithBehavior<Pump>()
        .Complete();
    
    /// <summary>
    ///     This is a magical pulsating block.
    /// </summary>
    public Block Pulsating { get; } = builder
        .BuildSimpleBlock(Language.PulsatingBlock, nameof(Pulsating))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("pulsating")))
        .WithBehavior<Paintable>()
        .WithBehavior<Animated>()
        .Complete();
    
    /// <summary>
    ///     The eternal flame, once lit, will never go out naturally.
    /// </summary>
    public Block EternalFlame { get; } = builder
        .BuildSimpleBlock(Language.EternalFlame, nameof(EternalFlame))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("eternal_flame")))
        .WithBehavior<EternallyBurning>()
        .Complete();
}
