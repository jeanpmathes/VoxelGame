// <copyright file="StoredHeight16.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Height;

/// <summary>
/// Defines the partial block height of a block as a stored attribute with 16 different states.
/// </summary>
/// <seealso cref="PartialHeight"/>
public class StoredHeight16 : BlockBehavior, IBehavior<StoredHeight16, BlockBehavior, Block>
{
    private IAttribute<Int32> Height => height ?? throw Exceptions.NotInitialized(nameof(height));
    private IAttribute<Int32>? height;
    
    /// <summary>
    /// The preferred height of the block at placement.
    /// </summary>
    public Int32 PlacementHeight { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="PlacementHeight"/> property.
    /// </summary>
    public Aspect<Int32, Block> PlacementHeightInitializer { get; }
    
    private StoredHeight16(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, state) => state.Get(Height), exclusive: true);
        
        PlacementHeightInitializer = Aspect<Int32, Block>.New<Exclusive<Int32, Block>>(nameof(PlacementHeightInitializer), this);
    }

    /// <inheritdoc/>
    public static StoredHeight16 Construct(Block input)
    {
        return new StoredHeight16(input);
    }

    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        height = builder
            .Define(nameof(height))
            .Int32(PartialHeight.MinimumHeight, PartialHeight.MaximumHeight + 1)
            .Attribute(generationDefault: PartialHeight.MaximumHeight);
    }
    
    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        PlacementHeight = PlacementHeightInitializer.GetValue(original: 0, Subject);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Modifiable.ModifyHeightMessage>(OnModifyHeight);
    }
    
    /// <inheritdoc/>
    protected override void OnValidate(IValidator validator)
    {
        if (PlacementHeight is >= PartialHeight.MinimumHeight and <= PartialHeight.MaximumHeight) 
            return;

        validator.ReportWarning("Placement height is out of bounds");
        PlacementHeight = PartialHeight.MinimumHeight;
    }

    private void OnModifyHeight(Modifiable.ModifyHeightMessage message)
    {
        State state = message.State;
        
        Int32 newHeight = (state.Get(Height) + 1) % (PartialHeight.MaximumHeight + 1);
        state.Set(Height, newHeight);
        
        message.World.SetBlock(new BlockInstance(state), message.Position);
    }
}
