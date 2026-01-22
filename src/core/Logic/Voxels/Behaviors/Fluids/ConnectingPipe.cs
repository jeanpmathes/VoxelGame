// <copyright file="ConnectingPipe.cs" company="VoxelGame">
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
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Siding;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     A <see cref="Pipe" /> which connects to other pipes.
/// </summary>
public partial class ConnectingPipe : BlockBehavior, IBehavior<ConnectingPipe, BlockBehavior, Block>
{
    private readonly Piped piped;
    private readonly StoredMultiSided siding;

    [Constructible]
    private ConnectingPipe(Block subject) : base(subject)
    {
        siding = subject.Require<StoredMultiSided>();

        piped = subject.Require<Piped>();

        subject.Require<Pipe>().OpenSides.ContributeFunction(GetOpenSides);

        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    /// <summary>
    ///     The models used for the block.
    /// </summary>
    public ResolvedProperty<(RID center, RID connector, RID surface)> Models { get; } = ResolvedProperty<(RID, RID, RID)>.New<Exclusive<(RID, RID, RID), Void>>(nameof(Models));

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Models.Initialize(this);
    }

    private Sides GetOpenSides(Sides original, State state)
    {
        return siding.GetSides(state);
    }

    private Mesh GetMesh(Mesh original, MeshContext context)
    {
        Model center = context.ModelProvider.GetModel(Models.Get().center);

        Model frontConnector = context.ModelProvider.GetModel(Models.Get().connector);
        Model frontSurface = context.ModelProvider.GetModel(Models.Get().surface);

        (Model front, Model back, Model left, Model right, Model bottom, Model top)
            connectors = Core.Visuals.Models.CreateModelsForAllSides(frontConnector, Model.TransformationMode.Reshape);

        (Model front, Model back, Model left, Model right, Model bottom, Model top)
            surfaces = Core.Visuals.Models.CreateModelsForAllSides(frontSurface, Model.TransformationMode.Reshape);

        Sides sides = siding.GetSides(context.State);

        return Model.Combine(center,
                sides.HasFlag(Sides.Front) ? connectors.front : surfaces.front,
                sides.HasFlag(Sides.Back) ? connectors.back : surfaces.back,
                sides.HasFlag(Sides.Left) ? connectors.left : surfaces.left,
                sides.HasFlag(Sides.Right) ? connectors.right : surfaces.right,
                sides.HasFlag(Sides.Bottom) ? connectors.bottom : surfaces.bottom,
                sides.HasFlag(Sides.Top) ? connectors.top : surfaces.top)
            .CreateMesh(context.TextureIndexProvider, Subject.Get<TextureOverride>()?.Textures.Get());
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        List<BoundingVolume> connectors = new(capacity: 6);

        Double diameter = Piped.GetPipeDiameter(piped.Tier.Get());

        Sides sides = siding.GetSides(state);
        Double connectorWidth = (0.5 - diameter) / 2.0;

        foreach (Side side in Side.All.Sides())
        {
            if (!sides.HasFlag(side.ToFlag())) continue;

            var direction = (Vector3d) side.Direction();

            connectors.Add(
                new BoundingVolume(
                    (0.5, 0.5, 0.5) + direction * (0.5 - connectorWidth),
                    (diameter, diameter, diameter) + direction.Absolute() * (connectorWidth - diameter)));
        }

        return new BoundingVolume(
            new Vector3d(x: 0.5, y: 0.5, z: 0.5),
            new Vector3d(diameter, diameter, diameter),
            connectors.ToArray());
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;

        Sides sides = DetermineOpenSides(world, position);

        return siding.SetSides(original, sides);
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        Sides sides = DetermineOpenSides(message.World, message.Position);

        if (sides == siding.GetSides(message.State))
            return;

        State newState = siding.SetSides(message.State, sides);
        message.World.SetBlock(newState, message.Position);
    }

    private static Sides DetermineOpenSides(World world, Vector3i position)
    {
        var sides = Sides.None;

        foreach (Side side in Side.All.Sides())
        {
            Vector3i otherPosition = position.Offset(side);
            State? otherBlock = world.GetBlock(otherPosition);

            if (otherBlock?.Block.Get<Piped>() is {} otherPiped
                && otherPiped.CanConnect(otherBlock.Value, side.Opposite(), otherPiped.Tier.Get())) sides |= side.ToFlag();
        }

        if (sides.Count() == 1)
            sides = sides.Single() switch
            {
                Side.Front or Side.Back => Sides.Front | Sides.Back,
                Side.Left or Side.Right => Sides.Left | Sides.Right,
                Side.Top or Side.Bottom => Sides.Top | Sides.Bottom,
                Side.All => Sides.All,
                _ => throw Exceptions.UnsupportedEnumValue(sides)
            };

        return sides;
    }
}
