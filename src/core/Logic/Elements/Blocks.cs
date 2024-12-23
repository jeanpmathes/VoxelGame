﻿// <copyright file="Blocks.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Blocks;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Elements;

#pragma warning disable S1192 // Definition class.
#pragma warning disable S104 // Definition class.

/// <summary>
///     Contains all block definitions of the core game.
/// </summary>
public partial class Blocks
{
    /// <summary>
    ///     The maximum amount of different blocks that can be registered.
    /// </summary>
    private const Int32 BlockLimit = 1 << Section.DataShift;

    private readonly List<Block> blockList = [];
    private readonly Dictionary<String, Block> namedBlockDictionary = new();

    private Blocks(ITextureIndexProvider indexProvider, VisualConfiguration visuals, ILoadingContext loadingContext)
    {
        using (loadingContext.BeginStep("Block Loading"))
        {
            List<Block> allBlocks = [];

            Block Register(Block block)
            {
                allBlocks.Add(block);

                return block;
            }

            Air = Register(new AirBlock(Language.Air, nameof(Air)));

            Grass = Register(new GrassBlock(
                Language.Grass,
                nameof(Grass),
                TextureLayout.UniqueColumn("grass_side", "dirt", "grass"),
                TextureLayout.UniqueColumn("grass_side_wet", "dirt_wet", "grass_wet")));

            GrassBurned = Register(new CoveredGrassSpreadableBlock(
                Language.AshCoveredDirt,
                nameof(GrassBurned),
                TextureLayout.UniqueColumn("ash_side", "dirt", "ash"),
                hasNeutralTint: false));

            Dirt = Register(new DirtBlock(
                Language.Dirt,
                nameof(Dirt),
                TextureLayout.Uniform("dirt"),
                TextureLayout.Uniform("dirt_wet")));

            Farmland = Register(new InsetDirtBlock(
                Language.Farmland,
                nameof(Farmland),
                TextureLayout.UniqueTop("dirt", "farmland"),
                TextureLayout.UniqueTop("dirt_wet", "farmland_wet"),
                supportsFullGrowth: true));

            TallGrass = Register(new CrossPlantBlock(
                Language.TallGrass,
                nameof(TallGrass),
                "tall_grass",
                BlockFlags.Replaceable,
                BoundingVolume.CrossBlock));

            VeryTallGrass = Register(new DoubleCrossPlantBlock(
                Language.VeryTallGrass,
                nameof(VeryTallGrass),
                "very_tall_grass",
                topTexOffset: 1,
                BoundingVolume.CrossBlock));

            Flower = Register(new CrossPlantBlock(
                Language.Flower,
                nameof(Flower),
                "flower",
                BlockFlags.Replaceable,
                new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.175f, y: 0.5f, z: 0.175f))));

            TallFlower = Register(new DoubleCrossPlantBlock(
                Language.TallFlower,
                nameof(TallFlower),
                "tall_flower",
                topTexOffset: 1,
                BoundingVolume.CrossBlock));

            Mud = Register(new MudBlock(
                Language.Mud,
                nameof(Mud),
                TextureLayout.Uniform("mud"),
                maxVelocity: 0.1f));

            Pumice = Register(new BasicBlock(
                Language.Pumice,
                nameof(Pumice),
                BlockFlags.Basic,
                TextureLayout.Uniform("pumice")));

            Obsidian = Register(new BasicBlock(
                Language.Obsidian,
                nameof(Obsidian),
                BlockFlags.Basic,
                TextureLayout.Uniform("obsidian")));

            Snow = Register(new SnowBlock(
                Language.Snow,
                nameof(Snow),
                TextureLayout.Uniform("snow")));

            Leaves = Register(new NaturalBlock(
                Language.Leaves,
                nameof(Leaves),
                hasNeutralTint: true,
                new BlockFlags
                {
                    IsSolid = true,
                    RenderFaceAtNonOpaques = true
                },
                TextureLayout.Uniform("leaves")));

            Log = Register(new RotatedBlock(
                Language.Log,
                nameof(Log),
                BlockFlags.Basic,
                TextureLayout.Column("log:0", "log:1")));

            Wood = Register(new OrganicConstructionBlock(
                Language.Wood,
                nameof(Wood),
                TextureLayout.Uniform("wood")));

            Sand = Register(new PermeableBlock(
                Language.Sand,
                nameof(Sand),
                TextureLayout.Uniform("sand")));

            Gravel = Register(new PermeableBlock(
                Language.Gravel,
                nameof(Gravel),
                TextureLayout.Uniform("gravel")));

            Ash = Register(new BasicBlock(
                Language.Ash,
                nameof(Ash),
                BlockFlags.Basic,
                TextureLayout.Uniform("ash")));

            Cactus = Register(new GrowingBlock(
                Language.Cactus,
                nameof(Cactus),
                TextureLayout.Column("cactus:0", "cactus:1"),
                Sand,
                maxHeight: 4));

            Pumpkin = Register(new GroundedBlock(
                Language.Pumpkin,
                nameof(Pumpkin),
                BlockFlags.Basic,
                TextureLayout.Column("pumpkin:0", "pumpkin:1")));

            Melon = Register(new GroundedBlock(
                Language.Melon,
                nameof(Melon),
                BlockFlags.Basic,
                TextureLayout.Column("melon:0", "melon:1")));

            Spiderweb = Register(new SpiderWebBlock(
                Language.SpiderWeb,
                nameof(Spiderweb),
                "spider_web",
                maxVelocity: 0.01f));

            Vines = Register(new GrowingFlatBlock(
                Language.Vines,
                nameof(Vines),
                "vines",
                climbingVelocity: 2f,
                slidingVelocity: 1f));

            Flax = Register(new CropBlock(
                Language.Flax,
                nameof(Flax),
                "flax",
                second: 0,
                third: 1,
                fourth: 2,
                fifth: 3,
                sixth: 3,
                final: 4,
                dead: 5));

            Potatoes = Register(new CropBlock(
                Language.Potatoes,
                nameof(Potatoes),
                "potato",
                second: 1,
                third: 1,
                fourth: 2,
                fifth: 2,
                sixth: 3,
                final: 4,
                dead: 5));

            Onions = Register(new CropBlock(
                Language.Onions,
                nameof(Onions),
                "onion",
                second: 0,
                third: 1,
                fourth: 1,
                fifth: 2,
                sixth: 2,
                final: 3,
                dead: 4));

            Wheat = Register(new CropBlock(
                Language.Wheat,
                nameof(Wheat),
                "wheat",
                second: 0,
                third: 1,
                fourth: 1,
                fifth: 2,
                sixth: 2,
                final: 3,
                dead: 4));

            Maize = Register(new DoubleCropBlock(
                Language.Maize,
                nameof(Maize),
                "maize",
                dead: 0,
                first: 1,
                second: 2,
                third: 2,
                (3, 6),
                (3, 6),
                (4, 7),
                (5, 8)));

            PumpkinPlant = Register(new FruitCropBlock(
                Language.PumpkinPlant,
                nameof(PumpkinPlant),
                "pumpkin_plant",
                Pumpkin));

            MelonPlant = Register(new FruitCropBlock(
                Language.MelonPlant,
                nameof(MelonPlant),
                "melon_plant",
                Melon));

            Glass = Register(new GlassBlock(
                Language.Glass,
                nameof(Glass),
                TextureLayout.Uniform("glass")));

            GlassTiled = Register(new GlassBlock(
                Language.TiledGlass,
                nameof(GlassTiled),
                TextureLayout.Uniform("glass_tiled")));

            Steel = Register(new ConstructionBlock(
                Language.Steel,
                nameof(Steel),
                TextureLayout.Uniform("steel")));

            Ladder = Register(new FlatBlock(
                Language.Ladder,
                nameof(Ladder),
                "ladder",
                climbingVelocity: 3f,
                slidingVelocity: 1f));

            TilesSmall = Register(new ConstructionBlock(
                Language.SmallTiles,
                nameof(TilesSmall),
                TextureLayout.Uniform("small_tiles")));

            TilesLarge = Register(new ConstructionBlock(
                Language.LargeTiles,
                nameof(TilesLarge),
                TextureLayout.Uniform("large_tiles")));

            TilesCheckerboardBlack = Register(new TintedBlock(
                Language.CheckerboardTilesBlack,
                nameof(TilesCheckerboardBlack),
                BlockFlags.Basic,
                TextureLayout.Uniform("checkerboard_tiles_black")));

            TilesCheckerboardWhite = Register(new TintedBlock(
                Language.CheckerboardTilesWhite,
                nameof(TilesCheckerboardWhite),
                BlockFlags.Basic,
                TextureLayout.Uniform("checkerboard_tiles_white")));

            ClayBricks = Register(new ConstructionBlock(
                Language.ClayBricks,
                nameof(ClayBricks),
                TextureLayout.Uniform("bricks")));

            RedPlastic = Register(new ConstructionBlock(
                Language.RedPlastic,
                nameof(RedPlastic),
                TextureLayout.Uniform("red_plastic")));

            Concrete = Register(new ConcreteBlock(
                Language.Concrete,
                nameof(Concrete),
                TextureLayout.Uniform("concrete")));

            Vase = Register(new CustomModelBlock(
                Language.Vase,
                nameof(Vase),
                BlockFlags.Basic,
                "vase",
                new BoundingVolume(new Vector3d(x: 0.5f, y: 0.375f, z: 0.5f), new Vector3d(x: 0.25f, y: 0.375f, z: 0.25f))));

            Bed = Register(new BedBlock(Language.Bed, nameof(Bed), "bed"));

            Wool = Register(new OrganicTintedBlock(
                Language.Wool,
                nameof(Wool),
                TextureLayout.Uniform("wool")));

            WoolDecorated = Register(new OrganicTintedBlock(
                Language.WoolDecorated,
                nameof(WoolDecorated),
                TextureLayout.Uniform("wool_decorated")));

            Carpet = Register(new TintedCustomModelBlock(
                Language.Carpet,
                nameof(Carpet),
                BlockFlags.Basic,
                "carpet",
                new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f))));

            CarpetDecorated = Register(new TintedCustomModelBlock(
                Language.CarpetDecorated,
                nameof(CarpetDecorated),
                BlockFlags.Basic,
                "carpet_decorated",
                new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f))));

            GlassPane = Register(new ThinConnectingBlock(
                Language.GlassPane,
                nameof(GlassPane),
                isOpaque: false,
                "pane_glass_post",
                "pane_glass_side",
                "pane_glass_extension"));

            Bars = Register(new ThinConnectingBlock(
                Language.Bars,
                nameof(Bars),
                isOpaque: true,
                "bars_post",
                "bars_side",
                "bars_extension"));

            FenceWood = Register(new FenceBlock(
                Language.WoodenFence,
                nameof(FenceWood),
                "wood",
                "fence_post",
                "fence_extension"));

            ClayBrickWall = Register(new WallBlock(
                Language.ClayBrickWall,
                nameof(ClayBrickWall),
                "bricks",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            DoorSteel = Register(new DoorBlock(
                Language.SteelDoor,
                nameof(DoorSteel),
                "door_steel_closed",
                "door_steel_open"));

            DoorWood = Register(new OrganicDoorBlock(
                Language.WoodenDoor,
                nameof(DoorWood),
                "door_wood_closed",
                "door_wood_open"));

            GateWood = Register(new GateBlock(
                Language.WoodenGate,
                nameof(GateWood),
                "gate_wood_closed",
                "gate_wood_open"));

            FluidBarrier = Register(new FluidBarrierBlock(
                Language.Barrier,
                nameof(FluidBarrier),
                TextureLayout.Uniform("fluid_barrier_closed"),
                TextureLayout.Uniform("fluid_barrier_open")));

            SteelPipe = Register(new PipeBlock<IIndustrialPipeConnectable>(
                Language.SteelPipe,
                nameof(SteelPipe),
                diameter: 0.375f,
                "steel_pipe_center",
                "steel_pipe_connector",
                "steel_pipe_surface"));

            WoodenPipe = Register(new PipeBlock<IPrimitivePipeConnectable>(
                Language.WoodenPipe,
                nameof(WoodenPipe),
                diameter: 0.3125f,
                "wood_pipe_center",
                "wood_pipe_connector",
                "wood_pipe_surface"));

            StraightSteelPipe = Register(new StraightSteelPipeBlock(
                Language.SteelPipeStraight,
                nameof(StraightSteelPipe),
                diameter: 0.375f,
                "steel_pipe_straight"));

            PipeValve = Register(new SteelPipeValveBlock(
                Language.ValvePipe,
                nameof(PipeValve),
                diameter: 0.375f,
                "steel_pipe_valve_open",
                "steel_pipe_valve_closed"));

            Pump = Register(new PumpBlock(
                Language.Pump,
                nameof(Pump),
                pumpDistance: 16,
                TextureLayout.Uniform("pump")));

            Fire = Register(new FireBlock(
                Language.Fire,
                nameof(Fire),
                "fire_complete",
                "fire_side",
                "fire_top"));

            Pulsating = Register(new TintedBlock(
                Language.PulsatingBlock,
                nameof(Pulsating),
                BlockFlags.Basic,
                TextureLayout.Uniform("pulsating"),
                isAnimated: true));

            EternalFlame = Register(new EternalFlame(
                Language.EternalFlame,
                nameof(EternalFlame),
                TextureLayout.Uniform("eternal_flame")));

            Path = Register(new InsetDirtBlock(
                Language.Path,
                nameof(Path),
                TextureLayout.Uniform("dirt"),
                TextureLayout.Uniform("dirt_wet"),
                supportsFullGrowth: false));

            Granite = Register(new BasicBlock(
                Language.Granite,
                nameof(Granite),
                BlockFlags.Basic,
                TextureLayout.Uniform("granite")));

            Sandstone = Register(new BasicBlock(
                Language.Sandstone,
                nameof(Sandstone),
                BlockFlags.Basic,
                TextureLayout.Uniform("sandstone")));

            Limestone = Register(new BasicBlock(
                Language.Limestone,
                nameof(Limestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("limestone")));

            Marble = Register(new BasicBlock(
                Language.Marble,
                nameof(Marble),
                BlockFlags.Basic,
                TextureLayout.Uniform("marble")));

            Clay = Register(new BasicBlock(
                Language.Clay,
                nameof(Clay),
                BlockFlags.Basic,
                TextureLayout.Uniform("clay")));

            Permafrost = Register(new BasicBlock(
                Language.Permafrost,
                nameof(Permafrost),
                BlockFlags.Basic,
                TextureLayout.Uniform("permafrost")));

            Core = Register(new BasicBlock(
                Language.Core,
                nameof(Core),
                BlockFlags.Basic,
                TextureLayout.Uniform("core")));

            Ice = Register(new ModifiableHeightBlock(
                Language.Ice,
                nameof(Ice),
                TextureLayout.Uniform("ice")));

            Error = Register(new BasicBlock(
                Language.Error,
                nameof(Error),
                BlockFlags.Basic,
                TextureLayout.Uniform("missing_texture")));

            Roots = Register(new PermeableNaturalBlock(
                Language.Roots,
                nameof(Roots),
                hasNeutralTint: false,
                BlockFlags.Basic,
                TextureLayout.Uniform("roots")));

            Salt = Register(new SaltBlock(
                Language.Salt,
                nameof(Salt),
                TextureLayout.Uniform("salt")));

            WorkedGranite = Register(new BasicBlock(
                Language.GraniteWorked,
                nameof(WorkedGranite),
                BlockFlags.Basic,
                TextureLayout.Uniform("granite_worked")));

            WorkedSandstone = Register(new BasicBlock(
                Language.SandstoneWorked,
                nameof(WorkedSandstone),
                BlockFlags.Basic,
                TextureLayout.Uniform("sandstone_worked")));

            WorkedLimestone = Register(new BasicBlock(
                Language.LimestoneWorked,
                nameof(WorkedLimestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("limestone_worked")));

            WorkedMarble = Register(new BasicBlock(
                Language.MarbleWorked,
                nameof(WorkedMarble),
                BlockFlags.Basic,
                TextureLayout.Uniform("marble_worked")));

            WorkedPumice = Register(new BasicBlock(
                Language.PumiceWorked,
                nameof(WorkedPumice),
                BlockFlags.Basic,
                TextureLayout.Uniform("pumice_worked")));

            WorkedObsidian = Register(new BasicBlock(
                Language.ObsidianWorked,
                nameof(WorkedObsidian),
                BlockFlags.Basic,
                TextureLayout.Uniform("obsidian_worked")));

            DecoratedGranite = Register(new OrientedBlock(
                Language.GraniteDecorated,
                nameof(DecoratedGranite),
                BlockFlags.Basic,
                TextureLayout.UniqueFront("granite_worked_decorated", "granite_worked")));

            DecoratedSandstone = Register(new OrientedBlock(
                Language.SandstoneDecorated,
                nameof(DecoratedSandstone),
                BlockFlags.Basic,
                TextureLayout.UniqueFront("sandstone_worked_decorated", "sandstone_worked")));

            DecoratedLimestone = Register(new OrientedBlock(
                Language.LimestoneDecorated,
                nameof(DecoratedLimestone),
                BlockFlags.Basic,
                TextureLayout.UniqueFront("limestone_worked_decorated", "limestone_worked")));

            DecoratedMarble = Register(new OrientedBlock(
                Language.MarbleDecorated,
                nameof(DecoratedMarble),
                BlockFlags.Basic,
                TextureLayout.UniqueFront("marble_worked_decorated", "marble_worked")));

            DecoratedPumice = Register(new OrientedBlock(
                Language.PumiceDecorated,
                nameof(DecoratedPumice),
                BlockFlags.Basic,
                TextureLayout.UniqueFront("pumice_worked_decorated", "pumice_worked")));

            DecoratedObsidian = Register(new OrientedBlock(
                Language.ObsidianDecorated,
                nameof(DecoratedObsidian),
                BlockFlags.Basic,
                TextureLayout.UniqueFront("obsidian_worked_decorated", "obsidian_worked")));

            GraniteCobblestone = Register(new BasicBlock(
                Language.GraniteCobbles,
                nameof(GraniteCobblestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("granite_cobbles")));

            SandstoneCobblestone = Register(new BasicBlock(
                Language.SandstoneCobbles,
                nameof(SandstoneCobblestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("sandstone_cobbles")));

            LimestoneCobblestone = Register(new BasicBlock(
                Language.LimestoneCobbles,
                nameof(LimestoneCobblestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("limestone_cobbles")));

            MarbleCobblestone = Register(new BasicBlock(
                Language.MarbleCobbles,
                nameof(MarbleCobblestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("marble_cobbles")));

            PumiceCobblestone = Register(new BasicBlock(
                Language.PumiceCobbles,
                nameof(PumiceCobblestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("pumice_cobbles")));

            ObsidianCobblestone = Register(new BasicBlock(
                Language.ObsidianCobbles,
                nameof(ObsidianCobblestone),
                BlockFlags.Basic,
                TextureLayout.Uniform("obsidian_cobbles")));

            GranitePaving = Register(new BasicBlock(
                Language.GranitePaving,
                nameof(GranitePaving),
                BlockFlags.Basic,
                TextureLayout.Uniform("granite_paving")));

            SandstonePaving = Register(new BasicBlock(
                Language.SandstonePaving,
                nameof(SandstonePaving),
                BlockFlags.Basic,
                TextureLayout.Uniform("sandstone_paving")));

            LimestonePaving = Register(new BasicBlock(
                Language.LimestonePaving,
                nameof(LimestonePaving),
                BlockFlags.Basic,
                TextureLayout.Uniform("limestone_paving")));

            MarblePaving = Register(new BasicBlock(
                Language.MarblePaving,
                nameof(MarblePaving),
                BlockFlags.Basic,
                TextureLayout.Uniform("marble_paving")));

            PumicePaving = Register(new BasicBlock(
                Language.PumicePaving,
                nameof(PumicePaving),
                BlockFlags.Basic,
                TextureLayout.Uniform("pumice_paving")));

            ObsidianPaving = Register(new BasicBlock(
                Language.ObsidianPaving,
                nameof(ObsidianPaving),
                BlockFlags.Basic,
                TextureLayout.Uniform("obsidian_paving")));

            GraniteRubble = Register(new PermeableBlock(
                Language.GraniteRubble,
                nameof(GraniteRubble),
                TextureLayout.Uniform("granite_rubble")));

            SandstoneRubble = Register(new PermeableBlock(
                Language.SandstoneRubble,
                nameof(SandstoneRubble),
                TextureLayout.Uniform("sandstone_rubble")));

            LimestoneRubble = Register(new PermeableBlock(
                Language.LimestoneRubble,
                nameof(LimestoneRubble),
                TextureLayout.Uniform("limestone_rubble")));

            MarbleRubble = Register(new PermeableBlock(
                Language.MarbleRubble,
                nameof(MarbleRubble),
                TextureLayout.Uniform("marble_rubble")));

            PumiceRubble = Register(new PermeableBlock(
                Language.PumiceRubble,
                nameof(PumiceRubble),
                TextureLayout.Uniform("pumice_rubble")));

            ObsidianRubble = Register(new PermeableBlock(
                Language.ObsidianRubble,
                nameof(ObsidianRubble),
                TextureLayout.Uniform("obsidian_rubble")));

            GraniteWall = Register(new WallBlock(
                Language.GraniteWall,
                nameof(GraniteWall),
                "granite_rubble",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            SandstoneWall = Register(new WallBlock(
                Language.SandstoneWall,
                nameof(SandstoneWall),
                "sandstone_rubble",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            LimestoneWall = Register(new WallBlock(
                Language.LimestoneWall,
                nameof(LimestoneWall),
                "limestone_rubble",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            MarbleWall = Register(new WallBlock(
                Language.MarbleWall,
                nameof(MarbleWall),
                "marble_rubble",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            PumiceWall = Register(new WallBlock(
                Language.PumiceWall,
                nameof(PumiceWall),
                "pumice_rubble",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            ObsidianWall = Register(new WallBlock(
                Language.ObsidianWall,
                nameof(ObsidianWall),
                "obsidian_rubble",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            GraniteBricks = Register(new ConstructionBlock(
                Language.GraniteBricks,
                nameof(GraniteBricks),
                TextureLayout.Uniform("granite_bricks")));

            SandstoneBricks = Register(new ConstructionBlock(
                Language.SandstoneBricks,
                nameof(SandstoneBricks),
                TextureLayout.Uniform("sandstone_bricks")));

            LimestoneBricks = Register(new ConstructionBlock(
                Language.LimestoneBricks,
                nameof(LimestoneBricks),
                TextureLayout.Uniform("limestone_bricks")));

            MarbleBricks = Register(new ConstructionBlock(
                Language.MarbleBricks,
                nameof(MarbleBricks),
                TextureLayout.Uniform("marble_bricks")));

            PumiceBricks = Register(new ConstructionBlock(
                Language.PumiceBricks,
                nameof(PumiceBricks),
                TextureLayout.Uniform("pumice_bricks")));

            ObsidianBricks = Register(new ConstructionBlock(
                Language.ObsidianBricks,
                nameof(ObsidianBricks),
                TextureLayout.Uniform("obsidian_bricks")));

            GraniteBrickWall = Register(new WallBlock(
                Language.GraniteBrickWall,
                nameof(GraniteBrickWall),
                "granite_bricks",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            SandstoneBrickWall = Register(new WallBlock(
                Language.SandstoneBrickWall,
                nameof(SandstoneBrickWall),
                "sandstone_bricks",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            LimestoneBrickWall = Register(new WallBlock(
                Language.LimestoneBrickWall,
                nameof(LimestoneBrickWall),
                "limestone_bricks",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            MarbleBrickWall = Register(new WallBlock(
                Language.MarbleBrickWall,
                nameof(MarbleBrickWall),
                "marble_bricks",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            PumiceBrickWall = Register(new WallBlock(
                Language.PumiceBrickWall,
                nameof(PumiceBrickWall),
                "pumice_bricks",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            ObsidianBrickWall = Register(new WallBlock(
                Language.ObsidianBrickWall,
                nameof(ObsidianBrickWall),
                "obsidian_bricks",
                "wall_post",
                "wall_extension",
                "wall_extension_straight"));

            GraniteColumn = Register(new ConstructionBlock(
                Language.GraniteColumn,
                nameof(GraniteColumn),
                TextureLayout.Column("granite_column", "granite_worked")));

            SandstoneColumn = Register(new ConstructionBlock(
                Language.SandstoneColumn,
                nameof(SandstoneColumn),
                TextureLayout.Column("sandstone_column", "sandstone_worked")));

            LimestoneColumn = Register(new ConstructionBlock(
                Language.LimestoneColumn,
                nameof(LimestoneColumn),
                TextureLayout.Column("limestone_column", "limestone_worked")));

            MarbleColumn = Register(new ConstructionBlock(
                Language.MarbleColumn,
                nameof(MarbleColumn),
                TextureLayout.Column("marble_column", "marble_worked")));

            PumiceColumn = Register(new ConstructionBlock(
                Language.PumiceColumn,
                nameof(PumiceColumn),
                TextureLayout.Column("pumice_column", "pumice_worked")));

            ObsidianColumn = Register(new ConstructionBlock(
                Language.ObsidianColumn,
                nameof(ObsidianColumn),
                TextureLayout.Column("obsidian_column", "obsidian_worked")));

            Lignite = Register(new BasicBlock(
                Language.CoalLignite,
                nameof(Lignite),
                BlockFlags.Basic,
                TextureLayout.Uniform("coal_lignite")));

            BituminousCoal = Register(new BasicBlock(
                Language.CoalBituminous,
                nameof(BituminousCoal),
                BlockFlags.Basic,
                TextureLayout.Uniform("coal_bituminous")));

            Anthracite = Register(new BasicBlock(
                Language.CoalAnthracite,
                nameof(Anthracite),
                BlockFlags.Basic,
                TextureLayout.Uniform("coal_anthracite")));

            Magnetite = Register(new BasicBlock(
                Language.OreMagnetite,
                nameof(Magnetite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_iron_magnetite")));

            Hematite = Register(new BasicBlock(
                Language.OreHematite,
                nameof(Hematite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_iron_hematite")));

            NativeGold = Register(new BasicBlock(
                Language.OreNativeGold,
                nameof(NativeGold),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_gold_native")));

            NativeSilver = Register(new BasicBlock(
                Language.OreNativeSilver,
                nameof(NativeSilver),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_silver_native")));

            NativePlatinum = Register(new BasicBlock(
                Language.OreNativePlatinum,
                nameof(NativePlatinum),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_platinum_native")));

            NativeCopper = Register(new BasicBlock(
                Language.OreNativeCopper,
                nameof(NativeCopper),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_copper_native")));

            Chalcopyrite = Register(new BasicBlock(
                Language.OreChalcopyrite,
                nameof(Chalcopyrite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_copper_chalcopyrite")));

            Malachite = Register(new BasicBlock(
                Language.OreMalachite,
                nameof(Malachite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_copper_malachite")));

            Electrum = Register(new BasicBlock(
                Language.OreElectrum,
                nameof(Electrum),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_electrum_native")));

            Bauxite = Register(new BasicBlock(
                Language.OreBauxite,
                nameof(Bauxite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_aluminium_bauxite")));

            Galena = Register(new BasicBlock(
                Language.OreGalena,
                nameof(Galena),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_lead_galena")));

            Cassiterite = Register(new BasicBlock(
                Language.OreCassiterite,
                nameof(Cassiterite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_tin_cassiterite")));

            Cinnabar = Register(new BasicBlock(
                Language.OreCinnabar,
                nameof(Cinnabar),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_mercury_cinnabar")));

            Sphalerite = Register(new BasicBlock(
                Language.OreSphalerite,
                nameof(Sphalerite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_zinc_sphalerite")));

            Chromite = Register(new BasicBlock(
                Language.OreChromite,
                nameof(Chromite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_chromium_chromite")));

            Pyrolusite = Register(new BasicBlock(
                Language.OrePyrolusite,
                nameof(Pyrolusite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_manganese_pyrolusite")));

            Rutile = Register(new BasicBlock(
                Language.OreRutile,
                nameof(Rutile),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_titanium_rutile")));

            Pentlandite = Register(new BasicBlock(
                Language.OrePentlandite,
                nameof(Pentlandite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_nickel_pentlandite")));

            Zircon = Register(new BasicBlock(
                Language.OreZircon,
                nameof(Zircon),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_zirconium_zircon")));

            Dolomite = Register(new BasicBlock(
                Language.OreDolomite,
                nameof(Dolomite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_magnesium_dolomite")));

            Celestine = Register(new BasicBlock(
                Language.OreCelestine,
                nameof(Celestine),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_strontium_celestine")));

            Uraninite = Register(new BasicBlock(
                Language.OreUraninite,
                nameof(Uraninite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_uranium_uraninite")));

            Bismuthinite = Register(new BasicBlock(
                Language.OreBismuthinite,
                nameof(Bismuthinite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_bismuth_bismuthinite")));

            Beryl = Register(new BasicBlock(
                Language.OreBeryl,
                nameof(Beryl),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_beryllium_beryl")));

            Molybdenite = Register(new BasicBlock(
                Language.OreMolybdenite,
                nameof(Molybdenite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_molybdenum_molybdenite")));

            Cobaltite = Register(new BasicBlock(
                Language.OreCobaltite,
                nameof(Cobaltite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_cobalt_cobaltite")));

            Spodumene = Register(new BasicBlock(
                Language.OreSpodumene,
                nameof(Spodumene),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_lithium_spodumene")));

            Vanadinite = Register(new BasicBlock(
                Language.OreVanadinite,
                nameof(Vanadinite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_vanadium_vanadinite")));

            Scheelite = Register(new BasicBlock(
                Language.OreScheelite,
                nameof(Scheelite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_tungsten_scheelite")));

            Greenockite = Register(new BasicBlock(
                Language.OreGreenockite,
                nameof(Greenockite),
                BlockFlags.Basic,
                TextureLayout.Uniform("ore_cadmium_greenockite")));

            Rust = Register(new BasicBlock(
                Language.Rust,
                nameof(Rust),
                BlockFlags.Basic,
                TextureLayout.Uniform("rust")));

            if (allBlocks.Count > BlockLimit)
                Debug.Fail($"Not more than {BlockLimit} blocks are allowed.");

            foreach (Block block in allBlocks.Take(BlockLimit))
            {
                blockList.Add(block);
                namedBlockDictionary.Add(block.NamedID, block);

                var id = (UInt32) (blockList.Count - 1);

                block.SetUp(id, indexProvider, visuals);

                loadingContext.ReportSuccess(nameof(Block), block.NamedID);
            }
        }

        Specials = new SpecialBlocks(this);
    }

    /// <summary>
    ///     Get the blocks instance. Only available after a call to <see cref="Load" />
    /// </summary>
    public static Blocks Instance { get; private set; } = null!;

    /// <summary>
    ///     Gets the count of registered blocks.
    /// </summary>
    public Int32 Count => blockList.Count;

    /// <summary>
    ///     Get special blocks as their actual block type.
    /// </summary>
    internal SpecialBlocks Specials { get; }

    /// <summary>
    ///     Translates a block ID to a reference to the block that has that ID. If the ID is not valid, air is returned.
    /// </summary>
    /// <param name="id">The ID of the block to return.</param>
    /// <returns>The block with the ID or air if the ID is not valid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block TranslateID(UInt32 id)
    {
        if (blockList.Count > id) return blockList[(Int32) id];

        LogUnknownID(logger, id, Air.NamedID);

        return Air;
    }

    /// <summary>
    ///     Translate a named ID to the block that has that ID.
    /// </summary>
    /// <param name="namedID">The named ID to translate.</param>
    /// <returns>The block, or null if no block with the ID exists.</returns>
    public Block? TranslateNamedID(String namedID)
    {
        namedBlockDictionary.TryGetValue(namedID, out Block? block);

        return block;
    }

    /// <summary>
    ///     Loads all blocks and sets them up.
    /// </summary>
    public static void Load(ITextureIndexProvider indexProvider, VisualConfiguration visuals, ILoadingContext loadingContext)
    {
        Instance = new Blocks(indexProvider, visuals, loadingContext);
    }

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    internal class SpecialBlocks(Blocks blocks)
    {
        public ConcreteBlock Concrete { get; } = (ConcreteBlock) blocks.Concrete;
        public SnowBlock Snow { get; } = (SnowBlock) blocks.Snow;
        public ModifiableHeightBlock Ice { get; } = (ModifiableHeightBlock) blocks.Ice;
        public RotatedBlock Log { get; } = (RotatedBlock) blocks.Log;
        public FlatBlock Vines { get; } = (FlatBlock) blocks.Vines;
        public SaltBlock Salt { get; } = (SaltBlock) blocks.Salt;
    }

    #region NATURAL BLOCKS

    /// <summary>
    ///     The air block that fills the world. Could also be interpreted as "no block".
    /// </summary>
    public Block Air { get; }

    /// <summary>
    ///     Dirt with some grass on top. Plants can be placed on top of this.
    ///     The grass can burn, creating ash.
    /// </summary>
    public Block Grass { get; }

    /// <summary>
    ///     Grass that was burned. Water can burn the ash away.
    /// </summary>
    public Block GrassBurned { get; }

    /// <summary>
    ///     Simple dirt. Grass next to it can spread over it.
    /// </summary>
    public Block Dirt { get; }

    /// <summary>
    ///     Tilled dirt that allows many plants to grow.
    ///     While plants can also grow on normal grass, this block allows full growth.
    /// </summary>
    public Block Farmland { get; }

    /// <summary>
    ///     A tall grassy plant. Fluids will destroy it, if the level is too high.
    /// </summary>
    public Block TallGrass { get; }

    /// <summary>
    ///     A much larger version of the normal tall grass.
    /// </summary>
    public Block VeryTallGrass { get; }

    /// <summary>
    ///     A simple flower.
    /// </summary>
    public Block Flower { get; }

    /// <summary>
    ///     A very tall flower.
    /// </summary>
    public Block TallFlower { get; }

    /// <summary>
    ///     Mud is created when water and dirt mix.
    /// </summary>
    public Block Mud { get; }

    /// <summary>
    ///     Pumice is created when lava rapidly cools down, while being in contact with a lot of water.
    /// </summary>
    public Block Pumice { get; }

    /// <summary>
    ///     Obsidian is a dark type of stone, that forms from lava.
    /// </summary>
    public Block Obsidian { get; }

    /// <summary>
    ///     Snow covers the ground, and can have different heights.
    /// </summary>
    public Block Snow { get; }

    /// <summary>
    ///     Leaves are transparent parts of the tree. They are flammable.
    /// </summary>
    public Block Leaves { get; }

    /// <summary>
    ///     Log is the unprocessed, wooden part of a tree. As it is made of wood, it is flammable.
    /// </summary>
    public Block Log { get; }

    /// <summary>
    ///     Processed wood that can be used as construction material. It is flammable.
    /// </summary>
    public Block Wood { get; }

    /// <summary>
    ///     Sand naturally forms and allows water to flow through it.
    /// </summary>
    public Block Sand { get; }

    /// <summary>
    ///     Gravel, which is made out of small pebbles, allows water to flow through it.
    /// </summary>
    public Block Gravel { get; }

    /// <summary>
    ///     Ahs is the remainder of burning processes.
    /// </summary>
    public Block Ash { get; }

    #endregion NATURAL BLOCKS

    #region PLANT BLOCKS

    /// <summary>
    ///     A cactus slowly grows upwards. It can only be placed on sand.
    /// </summary>
    public Block Cactus { get; }

    /// <summary>
    ///     Pumpkins are the fruit of the pumpkin plant. They have to be placed on solid ground.
    /// </summary>
    public Block Pumpkin { get; }

    /// <summary>
    ///     Melons are the fruit of the melon plant. They have to be placed on solid ground.
    /// </summary>
    public Block Melon { get; }

    /// <summary>
    ///     Spiderwebs slow the movement of entities and can be used to trap enemies.
    /// </summary>
    public Block Spiderweb { get; }

    /// <summary>
    ///     Vines grow downwards, and can hang freely. It is possible to climb them.
    /// </summary>
    public Block Vines { get; }

    /// <summary>
    ///     Flax is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Block Flax { get; }

    /// <summary>
    ///     Potatoes are a crop plant that grows on farmland. They requires water to fully grow.
    /// </summary>
    public Block Potatoes { get; }

    /// <summary>
    ///     Onions are a crop plant that grows on farmland. They requires water to fully grow.
    /// </summary>
    public Block Onions { get; }

    /// <summary>
    ///     Wheat is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Block Wheat { get; }

    /// <summary>
    ///     Maize is a crop plant that grows on farmland.
    ///     Maize grows two blocks high. It requires water to fully grow.
    /// </summary>
    public Block Maize { get; }

    /// <summary>
    ///     The pumpkin plant grows pumpkin fruits.
    /// </summary>
    public Block PumpkinPlant { get; }

    /// <summary>
    ///     The melon plant grows melon fruits.
    /// </summary>
    public Block MelonPlant { get; }

    #endregion PLANT BLOCKS

    #region BUILDING BLOCKS

    /// <summary>
    ///     Glass is transparent block.
    /// </summary>
    public Block Glass { get; }

    /// <summary>
    ///     Tiled glass is like glass, but made out of four tiles.
    /// </summary>
    public Block GlassTiled { get; }

    /// <summary>
    ///     The steel block is a metal construction block.
    /// </summary>
    public Block Steel { get; }

    /// <summary>
    ///     A ladder allows climbing up and down.
    /// </summary>
    public Block Ladder { get; }

    /// <summary>
    ///     Small tiles for construction of floors and walls.
    /// </summary>
    public Block TilesSmall { get; }

    /// <summary>
    ///     Large tiles for construction of floors and walls.
    /// </summary>
    public Block TilesLarge { get; }

    /// <summary>
    ///     Black checkerboard tiles come in different colors.
    /// </summary>
    public Block TilesCheckerboardBlack { get; }

    /// <summary>
    ///     White checkerboard tiles come in different colors.
    /// </summary>
    public Block TilesCheckerboardWhite { get; }

    /// <summary>
    ///     Clay bricks, placed as a block and connected with mortar.
    ///     This block is a construction material.
    /// </summary>
    public Block ClayBricks { get; }

    /// <summary>
    ///     Red plastic is a construction material.
    /// </summary>
    public Block RedPlastic { get; }

    /// <summary>
    ///     Concrete is a flexible construction material that can have different heights and colors.
    ///     It can be build using fluid concrete.
    /// </summary>
    public Block Concrete { get; }

    #endregion BUILDING BLOCKS

    #region DECORATION BLOCKS

    /// <summary>
    ///     The vase is a decorative block that must be placed on solid ground.
    /// </summary>
    public Block Vase { get; }

    /// <summary>
    ///     The bed can be placed to set a different spawn point.
    ///     It is possible to change to color of a bed.
    /// </summary>
    public Block Bed { get; }

    /// <summary>
    ///     Wool is a flammable material, that allows its color to be changed.
    /// </summary>
    public Block Wool { get; }

    /// <summary>
    ///     Decorated wool is similar to wool, decorated with golden ornaments.
    /// </summary>
    public Block WoolDecorated { get; }

    /// <summary>
    ///     Carpets can be used to cover the floor. Their color can be changed.
    /// </summary>
    public Block Carpet { get; }

    /// <summary>
    ///     Decorated carpets are similar to carpets, decorated with golden ornaments.
    /// </summary>
    public Block CarpetDecorated { get; }

    /// <summary>
    ///     Glass panes are a thin alternative to glass blocks.
    ///     They connect to some neighboring blocks.
    /// </summary>
    public Block GlassPane { get; }

    /// <summary>
    ///     Steel bars are a thin, but strong barrier.
    /// </summary>
    public Block Bars { get; }

    #endregion DECORATION BLOCKS

    #region ACCESS BLOCKS

    /// <summary>
    ///     The wooden fence can be used as way of marking areas. It does not prevent jumping over it.
    ///     As this fence is made out of wood, it is flammable. Fences can connect to other blocks.
    /// </summary>
    public Block FenceWood { get; }

    /// <summary>
    ///     A wall constructed using clay bricks.
    ///     The wall does not prevent jumping over it, and can connect to other blocks.
    /// </summary>
    public Block ClayBrickWall { get; }

    /// <summary>
    ///     The steel door allows closing of a room. It can be opened and closed.
    /// </summary>
    public Block DoorSteel { get; }

    /// <summary>
    ///     The wooden door allows closing of a room. It can be opened and closed.
    ///     As this door is made out of wood, it is flammable.
    /// </summary>
    public Block DoorWood { get; }

    /// <summary>
    ///     Fence gates are meant as a passage trough fences and walls.
    /// </summary>
    public Block GateWood { get; }

    #endregion ACCESS BLOCKS

    #region FLUID FLOW BLOCKS

    /// <summary>
    ///     The fluid barrier can be used to control fluid flow. It can be opened and closed.
    ///     It does not prevent gasses from flowing through it.
    /// </summary>
    public Block FluidBarrier { get; }

    /// <summary>
    ///     The industrial steel pipe can be used to control fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public Block SteelPipe { get; }

    /// <summary>
    ///     The wooden pipe offers a primitive way of controlling fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public Block WoodenPipe { get; }

    /// <summary>
    ///     This pipe is a special steel pipe that can only form straight connections.
    ///     It is ideal for parallel pipes.
    /// </summary>
    public Block StraightSteelPipe { get; }

    /// <summary>
    ///     This is a special steel pipe that can be closed. It prevents all fluid flow.
    /// </summary>
    public Block PipeValve { get; }

    /// <summary>
    ///     The pump can lift fluids up when interacted with.
    ///     It can only lift up to a threshold of 16 blocks.
    /// </summary>
    public Block Pump { get; }

    #endregion FLUID FLOW BLOCKS

    #region SPECIAL BLOCKS

    /// <summary>
    ///     Fire is a dangerous block that spreads onto nearby flammable blocks.
    ///     When spreading, fire burns blocks which can destroy them.
    /// </summary>
    public Block Fire { get; }

    /// <summary>
    ///     This is a magical pulsating block.
    /// </summary>
    public Block Pulsating { get; }

    /// <summary>
    ///     The eternal flame, once lit, will never go out naturally.
    /// </summary>
    public Block EternalFlame { get; }

    /// <summary>
    ///     The path is a dirt block with its top layer trampled.
    /// </summary>
    public Block Path { get; }

    #endregion SPECIAL BLOCKS

    #region NEW BLOCKS

    /// <summary>
    ///     Granite is found next to volcanic activity.
    /// </summary>
    public Block Granite { get; }

    /// <summary>
    ///     Sandstone is found all over the world and especially in the desert.
    /// </summary>
    public Block Sandstone { get; }

    /// <summary>
    ///     Limestone is found all over the world and especially in oceans.
    /// </summary>
    public Block Limestone { get; }

    /// <summary>
    ///     Marble is a rarer stone type.
    /// </summary>
    public Block Marble { get; }

    /// <summary>
    ///     Clay is found beneath the ground and blocks groundwater flow.
    /// </summary>
    public Block Clay { get; }

    /// <summary>
    ///     Permafrost is a type of soil that is frozen solid.
    /// </summary>
    public Block Permafrost { get; }

    /// <summary>
    ///     The core of the world, which is found at the lowest level.
    /// </summary>
    public Block Core { get; }

    /// <summary>
    ///     A block made out of frozen water.
    /// </summary>
    public Block Ice { get; }

    /// <summary>
    ///     An error block, used as fallback when structure operations fail.
    /// </summary>
    public Block Error { get; }

    /// <summary>
    ///     Roots grow at the bottom of trees.
    /// </summary>
    public Block Roots { get; }

    /// <summary>
    ///     Salt is contained in sea water, it becomes usable after the water evaporates.
    /// </summary>
    public Block Salt { get; }

    /// <summary>
    ///     Worked granite is a processed granite block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedGranite { get; }

    /// <summary>
    ///     Worked sandstone is a processed sandstone block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedSandstone { get; }

    /// <summary>
    ///     Worked limestone is a processed limestone block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedLimestone { get; }

    /// <summary>
    ///     Worked marble is a processed marble block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedMarble { get; }

    /// <summary>
    ///     Worked pumice is a processed pumice block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedPumice { get; }

    /// <summary>
    ///     Worked obsidian is a processed obsidian block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedObsidian { get; }

    /// <summary>
    ///     Worked granite with decorations carved into one side.
    ///     The carvings show a pattern of geometric shapes.
    /// </summary>
    public Block DecoratedGranite { get; }

    /// <summary>
    ///     Worked sandstone with decorations carved into one side.
    ///     The carvings depict the desert sun.
    /// </summary>
    public Block DecoratedSandstone { get; }

    /// <summary>
    ///     Worked limestone with decorations carved into one side.
    ///     The carvings show the ocean and life within it.
    /// </summary>
    public Block DecoratedLimestone { get; }

    /// <summary>
    ///     Worked marble with decorations carved into one side.
    ///     The carvings depict an ancient temple.
    /// </summary>
    public Block DecoratedMarble { get; }

    /// <summary>
    ///     Worked pumice with decorations carved into one side.
    ///     The carvings depict heat rising from the earth.
    /// </summary>
    public Block DecoratedPumice { get; }

    /// <summary>
    ///     Worked obsidian with decorations carved into one side.
    ///     The carvings depict an ancient artifact.
    /// </summary>
    public Block DecoratedObsidian { get; }

    /// <summary>
    ///     Marble cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block GraniteCobblestone { get; }

    /// <summary>
    ///     Sandstone cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block SandstoneCobblestone { get; }

    /// <summary>
    ///     Limestone cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block LimestoneCobblestone { get; }

    /// <summary>
    ///     Marble cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block MarbleCobblestone { get; }

    /// <summary>
    ///     Pumice cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block PumiceCobblestone { get; }

    /// <summary>
    ///     Obsidian cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block ObsidianCobblestone { get; }

    /// <summary>
    ///     Paving made out of processed granite.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block GranitePaving { get; }

    /// <summary>
    ///     Paving made out of processed sandstone.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block SandstonePaving { get; }

    /// <summary>
    ///     Paving made out of processed limestone.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block LimestonePaving { get; }

    /// <summary>
    ///     Paving made out of processed marble.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block MarblePaving { get; }

    /// <summary>
    ///     Paving made out of processed pumice.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block PumicePaving { get; }

    /// <summary>
    ///     Paving made out of processed obsidian.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block ObsidianPaving { get; }

    /// <summary>
    ///     When breaking granite, it turns into granite rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block GraniteRubble { get; }

    /// <summary>
    ///     When breaking sandstone, it turns into sandstone rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block SandstoneRubble { get; }

    /// <summary>
    ///     When breaking limestone, it turns into limestone rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block LimestoneRubble { get; }

    /// <summary>
    ///     When breaking marble, it turns into marble rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block MarbleRubble { get; }

    /// <summary>
    ///     When breaking pumice, it turns into pumice rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block PumiceRubble { get; }

    /// <summary>
    ///     When breaking obsidian, it turns into obsidian rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block ObsidianRubble { get; }

    /// <summary>
    ///     A wall made out of granite rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block GraniteWall { get; }

    /// <summary>
    ///     A wall made out of sandstone rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block SandstoneWall { get; }

    /// <summary>
    ///     A wall made out of limestone rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block LimestoneWall { get; }

    /// <summary>
    ///     A wall made out of marble rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block MarbleWall { get; }

    /// <summary>
    ///     A wall made out of pumice rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block PumiceWall { get; }

    /// <summary>
    ///     A wall made out of obsidian rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block ObsidianWall { get; }

    /// <summary>
    ///     Granite, cut into bricks and connected with mortar.
    /// </summary>
    public Block GraniteBricks { get; }

    /// <summary>
    ///     Sandstone, cut into bricks and connected with mortar.
    /// </summary>
    public Block SandstoneBricks { get; }

    /// <summary>
    ///     Limestone, cut into bricks and connected with mortar.
    /// </summary>
    public Block LimestoneBricks { get; }

    /// <summary>
    ///     Marble, cut into bricks and connected with mortar.
    /// </summary>
    public Block MarbleBricks { get; }

    /// <summary>
    ///     Pumice, cut into bricks and connected with mortar.
    /// </summary>
    public Block PumiceBricks { get; }

    /// <summary>
    ///     Obsidian, cut into bricks and connected with mortar.
    /// </summary>
    public Block ObsidianBricks { get; }

    /// <summary>
    ///     A wall constructed using granite bricks.
    /// </summary>
    public Block GraniteBrickWall { get; }

    /// <summary>
    ///     A wall constructed using sandstone bricks.
    /// </summary>
    public Block SandstoneBrickWall { get; }

    /// <summary>
    ///     A wall constructed using limestone bricks.
    /// </summary>
    public Block LimestoneBrickWall { get; }

    /// <summary>
    ///     A wall constructed using marble bricks.
    /// </summary>
    public Block MarbleBrickWall { get; }

    /// <summary>
    ///     A wall constructed using pumice bricks.
    /// </summary>
    public Block PumiceBrickWall { get; }

    /// <summary>
    ///     A wall constructed using obsidian bricks.
    /// </summary>
    public Block ObsidianBrickWall { get; }

    /// <summary>
    ///     A column, made out of granite.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block GraniteColumn { get; }

    /// <summary>
    ///     A column, made out of sandstone.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block SandstoneColumn { get; }

    /// <summary>
    ///     A column, made out of limestone.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block LimestoneColumn { get; }

    /// <summary>
    ///     A column, made out of marble.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block MarbleColumn { get; }

    /// <summary>
    ///     A column, made out of pumice.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block PumiceColumn { get; }

    /// <summary>
    ///     A column, made out of obsidian.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block ObsidianColumn { get; }

    /// <summary>
    ///     Lignite is a type of coal.
    ///     It is the lowest rank of coal but can be found near the surface.
    /// </summary>
    public Block Lignite { get; }

    /// <summary>
    ///     Bituminous coal is a type of coal.
    ///     It is of medium rank and is the most abundant type of coal.
    /// </summary>
    public Block BituminousCoal { get; }

    /// <summary>
    ///     Anthracite is a type of coal.
    ///     It is the highest rank of coal and is the hardest and most carbon-rich.
    /// </summary>
    public Block Anthracite { get; }

    /// <summary>
    ///     Magnetite is a type of iron ore.
    /// </summary>
    public Block Magnetite { get; }

    /// <summary>
    ///     Hematite is a type of iron ore.
    /// </summary>
    public Block Hematite { get; }

    /// <summary>
    ///     Native gold is gold ore, containing mostly gold with some impurities.
    /// </summary>
    public Block NativeGold { get; }

    /// <summary>
    ///     Native silver is silver ore, containing mostly silver with some impurities.
    /// </summary>
    public Block NativeSilver { get; }

    /// <summary>
    ///     Native platinum is platinum ore, containing mostly platinum with some impurities.
    /// </summary>
    public Block NativePlatinum { get; }

    /// <summary>
    ///     Native copper is copper ore, containing mostly copper with some impurities.
    /// </summary>
    public Block NativeCopper { get; }

    /// <summary>
    ///     Chalcopyrite is a copper ore.
    ///     It is the most abundant copper ore but is not as rich in copper as other ores.
    /// </summary>
    public Block Chalcopyrite { get; }

    /// <summary>
    ///     Malachite is a copper ore.
    ///     It is rich in copper, but is not as abundant as other ores.
    /// </summary>
    public Block Malachite { get; }

    /// <summary>
    ///     Electrum is a naturally occurring alloy of gold and silver.
    /// </summary>
    public Block Electrum { get; }

    /// <summary>
    ///     Bauxite is an aluminum ore.
    /// </summary>
    public Block Bauxite { get; }

    /// <summary>
    ///     Galena is a lead ore that is rich in lead and silver.
    /// </summary>
    public Block Galena { get; }

    /// <summary>
    ///     Cassiterite is a tin ore.
    /// </summary>
    public Block Cassiterite { get; }

    /// <summary>
    ///     Cinnabar is a mercury ore.
    /// </summary>
    public Block Cinnabar { get; }

    /// <summary>
    ///     Sphalerite is a zinc ore.
    /// </summary>
    public Block Sphalerite { get; }

    /// <summary>
    ///     Chromite is a chromium ore.
    /// </summary>
    public Block Chromite { get; }

    /// <summary>
    ///     Pyrolusite is a manganese ore.
    /// </summary>
    public Block Pyrolusite { get; }

    /// <summary>
    ///     Rutile is a titanium ore.
    /// </summary>
    public Block Rutile { get; }

    /// <summary>
    ///     Pentlandite is a nickel ore which is also rich in iron.
    /// </summary>
    public Block Pentlandite { get; }

    /// <summary>
    ///     Zircon is a zirconium ore.
    /// </summary>
    public Block Zircon { get; }

    /// <summary>
    ///     Dolomite is a carbonate rock, rich in magnesium.
    /// </summary>
    public Block Dolomite { get; }

    /// <summary>
    ///     Celestine is a strontium ore.
    /// </summary>
    public Block Celestine { get; }

    /// <summary>
    ///     Uraninite is a uranium ore.
    /// </summary>
    public Block Uraninite { get; }

    /// <summary>
    ///     Bismuthinite is a bismuth ore.
    /// </summary>
    public Block Bismuthinite { get; }

    /// <summary>
    ///     Beryl is a beryllium ore.
    ///     This generic beryl is of low grade in comparison to beryls like emerald and aquamarine.
    /// </summary>
    public Block Beryl { get; }

    /// <summary>
    ///     Molybdenite is a molybdenum ore.
    /// </summary>
    public Block Molybdenite { get; }

    /// <summary>
    ///     Cobaltite is a cobalt ore.
    /// </summary>
    public Block Cobaltite { get; }

    /// <summary>
    ///     Spodumene is a lithium ore.
    /// </summary>
    public Block Spodumene { get; }

    /// <summary>
    ///     Vanadinite is a vanadium ore.
    /// </summary>
    public Block Vanadinite { get; }

    /// <summary>
    ///     Scheelite is a tungsten ore.
    /// </summary>
    public Block Scheelite { get; }

    /// <summary>
    ///     Greenockite is a cadmium ore.
    /// </summary>
    public Block Greenockite { get; }

    /// <summary>
    ///     When iron is exposed to oxygen and moisture, it rusts.
    ///     This blocks is a large accumulation of rust.
    /// </summary>
    public Block Rust { get; }

    #endregion NEW BLOCKS

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Blocks>();

    [LoggerMessage(EventId = Events.UnknownBlock, Level = LogLevel.Warning, Message = "No Block with ID {ID} could be found, returning {Air} instead")]
    private static partial void LogUnknownID(ILogger logger, UInt32 id, String air);

    #endregion LOGGING
}
