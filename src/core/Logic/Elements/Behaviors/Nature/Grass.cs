// <copyright file="Grass.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// A special soil cover that spreads to blocks which are <see cref="GrassSpreadable"/>.
/// </summary>
public class Grass : BlockBehavior, IBehavior<Grass, BlockBehavior, Block>
{
    private Grass(Block subject) : base(subject)
    {
        subject.Require<CoveredSoil>();
        subject.Require<Combustible>();
    }
    
    /// <inheritdoc/>
    public static Grass Construct(Block input)
    {
        return new Grass(input);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.RandomUpdateMessage>(OnRandomUpdate);
        bus.Subscribe<Combustible.BurnMessage>(OnBurn);
    }
    
    private void OnRandomUpdate(Block.RandomUpdateMessage message)
    {
        for (Int32 yOffset = -1; yOffset <= 1; yOffset++)
            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i position = orientation.Offset(message.Position) + Vector3i.UnitY * yOffset;

                if (message.World.GetBlock(position)?.Block.Get<GrassSpreadable>() is {} spreadable)
                    spreadable.SpreadGrass(message.World, position, Subject);
            }
    }
    
    private static void OnBurn(Combustible.BurnMessage message)
    {
        message.World.SetBlock(new State(Blocks.Instance.Environment.AshCoveredSoil), message.Position);
        message.Fire.Place(message.World, message.Position.Above());
    }
}
