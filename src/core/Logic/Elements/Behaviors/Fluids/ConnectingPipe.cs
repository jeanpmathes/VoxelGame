// <copyright file="ConnectingPipe.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
///     A <see cref="Pipe" /> which connects to other pipes.
/// </summary>
public class ConnectingPipe : BlockBehavior, IBehavior<ConnectingPipe, BlockBehavior, Block>
{
    private readonly Piped piped;
    private readonly StoredMultiSided siding;

    private ConnectingPipe(Block subject) : base(subject)
    {
        siding = subject.Require<StoredMultiSided>();

        piped = subject.Require<Piped>();

        subject.Require<Pipe>().OpenSides.ContributeFunction(GetOpenSides);

        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);

        ModelsInitializer = Aspect<(RID, RID, RID), Block>.New<Exclusive<(RID, RID, RID), Block>>(nameof(ModelsInitializer), this);

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    /// <summary>
    ///     The models used for the block.
    /// </summary>
    public (RID center, RID connector, RID surface) Models { get; private set; }

    /// <summary>
    ///     Aspect used to initialize the <see cref="Models" /> property.
    /// </summary>
    public Aspect<(RID center, RID connector, RID surface), Block> ModelsInitializer { get; }

    /// <inheritdoc />
    public static ConnectingPipe Construct(Block input)
    {
        return new ConnectingPipe(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Models = ModelsInitializer.GetValue(original: default, Subject);
    }

    private Sides GetOpenSides(Sides original, State state)
    {
        return siding.GetSides(state);
    }

    private Mesh GetMesh(Mesh original, (State state, ITextureIndexProvider textureIndexProvider, IModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IModelProvider blockModelProvider, VisualConfiguration _) = context;

        Model center = blockModelProvider.GetModel(Models.center);

        Model frontConnector = blockModelProvider.GetModel(Models.connector);
        Model frontSurface = blockModelProvider.GetModel(Models.surface);

        (Model front, Model back, Model left, Model right, Model bottom, Model top)
            connectors = frontConnector.CreateAllSides();

        (Model front, Model back, Model left, Model right, Model bottom, Model top)
            surfaces = frontSurface.CreateAllSides();

        center.Lock(textureIndexProvider);
        connectors.Lock(textureIndexProvider);
        surfaces.Lock(textureIndexProvider);

        Sides sides = siding.GetSides(state);

        return Model.GetCombinedMesh(textureIndexProvider, // todo: use Subject.Get<TextureOverride>()?.Textures
            center,
            sides.HasFlag(Sides.Front) ? connectors.front : surfaces.front,
            sides.HasFlag(Sides.Back) ? connectors.back : surfaces.back,
            sides.HasFlag(Sides.Left) ? connectors.left : surfaces.left,
            sides.HasFlag(Sides.Right) ? connectors.right : surfaces.right,
            sides.HasFlag(Sides.Bottom) ? connectors.bottom : surfaces.bottom,
            sides.HasFlag(Sides.Top) ? connectors.top : surfaces.top);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        List<BoundingVolume> connectors = new(capacity: 6);

        Double diameter = Piped.GetPipeDiameter(piped.Tier);

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
            Vector3i otherPosition = side.Offset(position);
            State? otherBlock = world.GetBlock(otherPosition);

            if (otherBlock?.Block.Get<Piped>() is {} otherPiped
                && otherPiped.CanConnect(otherBlock.Value, side.Opposite(), otherPiped.Tier)) sides |= side.ToFlag();
        }

        if (sides.Count() == 1)
        {
            sides = sides.Single() switch
            {
                Side.Front or Side.Back => Sides.Front | Sides.Back,
                Side.Left or Side.Right => Sides.Left | Sides.Right,
                Side.Top or Side.Bottom => Sides.Top | Sides.Bottom,
                Side.All => Sides.All,
                _ => throw Exceptions.UnsupportedEnumValue(sides)
            };
        }

        return sides;
    }
}
