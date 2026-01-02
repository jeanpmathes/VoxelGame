// <copyright file="TallGrass.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     Provides the three height stages for tall grass and manages their transitions.
/// </summary>
public partial class TallGrass : BlockBehavior, IBehavior<TallGrass, BlockBehavior, Block>
{
    /// <summary>
    ///     The different height stages of tall grass.
    /// </summary>
    public enum StageState
    {
        /// <summary>
        ///     The shortest stage of tall grass.
        /// </summary>
        Short,

        /// <summary>
        ///     The intermediate stage of tall grass.
        /// </summary>
        Tall,

        /// <summary>
        ///     The tallest stage of tall grass, occupying two blocks.
        /// </summary>
        Tallest
    }

    private readonly Composite composite;

    [Constructible]
    private TallGrass(Block subject) : base(subject)
    {
        subject.Require<Plant>();

        subject.Require<VerticalTextureSelector>().HorizontalOffset.ContributeFunction(GetHorizontalOffset);

        composite = subject.Require<Composite>();
        composite.MaximumSize.Initializer.ContributeConstant((1, 2, 1));
        composite.Size.ContributeFunction(GetSize);

        var foliage = subject.Require<Foliage>();
        foliage.Layout.Initializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);
        foliage.Part.ContributeFunction(GetPart);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        subject.Replaceability.ContributeFunction(GetIsReplaceable);
    }

    [LateInitialization] private partial IAttributeData<StageState> Stage { get; set; }

    /// <summary>
    ///     The textures used for the individual stages.
    /// </summary>
    public ResolvedProperty<(TID Short, TID Tall, TID Tallest)> Textures { get; } = ResolvedProperty<(TID, TID, TID)>.New<Exclusive<(TID, TID, TID), Void>>(nameof(Textures));

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Textures.Initialize(this);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Stage = builder
            .Define(nameof(Stage))
            .Enum<StageState>()
            .Attribute(StageState.Short, StageState.Short);
    }

    private Int32 GetHorizontalOffset(Int32 original, State state)
    {
        StageState stage = state.Get(Stage);

        return stage switch
        {
            StageState.Short => 0,
            StageState.Tall => 1,
            StageState.Tallest => 2,
            _ => original
        };
    }

    private Vector3i GetSize(Vector3i original, State state)
    {
        return state.Get(Stage) == StageState.Tallest ? (1, 2, 1) : Vector3i.One;
    }

    private Foliage.PartType GetPart(Foliage.PartType original, State state)
    {
        StageState stage = state.Get(Stage);

        if (stage != StageState.Tallest)
            return Foliage.PartType.Single;

        return composite.GetPartPosition(state).Y == 0
            ? Foliage.PartType.DoubleLower
            : Foliage.PartType.DoubleUpper;
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Double height = state.Get(Stage) == StageState.Short ? 0.5 : 1.0;

        return BoundingVolume.CrossBlock(height, width: 0.71);
    }

    private Boolean GetIsReplaceable(Boolean original, State state)
    {
        return state.Get(Stage) != StageState.Tallest;
    }

    private void OnRandomUpdate(Block.IRandomUpdateMessage message)
    {
        if (composite.GetPartPosition(message.State).Y != 0)
            return;

        StageState stageState = message.State.Get(Stage);

        if (stageState == StageState.Tallest)
            return;

        StageState nextStageState = stageState switch
        {
            StageState.Short => StageState.Tall,
            StageState.Tall => StageState.Tallest,
            _ => stageState
        };

        message.World.SetBlock(SetStage(message.State, nextStageState), message.Position);
    }

    private State SetStage(State state, StageState stageState)
    {
        return state.With(Stage, stageState);
    }

    /// <summary>
    ///     Get the state of a tall grass block with the given stage.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="stageState">The desired stage.</param>
    /// <returns>The modified state, or the original state if the block is not tall grass.</returns>
    public static State GetState(State state, StageState stageState)
    {
        return state.Block.Get<TallGrass>() is {} tallGrass ? tallGrass.SetStage(state, stageState) : state;
    }
}
