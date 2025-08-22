// <copyright file="FruitCropPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;

/// <summary>
/// A crop <see cref="Plant"/> that uses the <see cref="Foliage.LayoutType.Cross"/> layout places a fruit when fully grown.
/// </summary>
public class FruitCropPlant : BlockBehavior, IBehavior<FruitCropPlant, BlockBehavior, Block>
{
    private readonly GrowingPlant plant;

    private Block fruit = null!;
    
    private IAttribute<Int32> Age => age ?? throw Exceptions.NotInitialized(nameof(age));
    private IAttribute<Int32>? age;

    private const Int32 MaxAge = 3;
    
    private FruitCropPlant(Block subject) : base(subject)
    {
        plant = subject.Require<GrowingPlant>();
        plant.StageCountInitializer.ContributeConstant(value: 2);
        
        var foliage = subject.Require<Foliage>();
        foliage.LayoutInitializer.ContributeConstant(Foliage.LayoutType.Crop, exclusive: true);
        foliage.Part.ContributeConstant(Foliage.PartType.Single);
        
        subject.Require<SingleTextured>().ActiveTexture.ContributeFunction(GetActiveTexture);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        
        FruitInitializer = Aspect<String?, Block>.New<Exclusive<String?, Block>>(nameof(FruitInitializer), this);
    }
    
    /// <summary>
    /// The fruit block.
    /// </summary>
    public String? Fruit { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Fruit"/> property.
    /// </summary>
    public Aspect<String?, Block> FruitInitializer { get; }

    /// <inheritdoc />
    public static FruitCropPlant Construct(Block input)
    {
        return new FruitCropPlant(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Fruit = FruitInitializer.GetValue(original: null, Subject);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        age = builder.Define(nameof(age)).Int32(min: 0, MaxAge + 1).Attribute();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<GrowingPlant.MatureUpdateMessage>(OnMatureUpdate);
    }
    
    /// <inheritdoc/>
    protected override void OnValidate(IResourceContext context)
    {
        if (Fruit == null)
            context.ReportWarning(this, "No fruit block is set");
        
        if (Fruit == Subject.NamedID)
            context.ReportWarning(this, "The fruit block cannot be the same as the growing block itself");
        
        fruit = Blocks.Instance.SafelyTranslateNamedID(Fruit);
        
        if (fruit == Blocks.Instance.Core.Error && Fruit != Blocks.Instance.Core.Error.NamedID)
            context.ReportWarning(this, $"The fruit block '{Fruit}' could not be found");
    }
    
    private void OnMatureUpdate(GrowingPlant.MatureUpdateMessage message)
    {
        Int32 currentAge = message.State.Get(Age);

        State newState = message.State;
        
        if (currentAge == MaxAge)
        {
            var placed = false;
            
            if (message.Ground.SupportsFullGrowth && message.Ground.TryGrow(
                    message.World,
                    message.Position.Below(),
                    Elements.Fluids.Instance.FreshWater,
                    FluidLevel.Two))
            {
                foreach (Utilities.Orientation orientation in Orientations.ShuffledStart(message.Position))
                {
                    if (!fruit.Place(message.World, orientation.Offset(message.Position))) 
                        continue;

                    placed = true;
                    break;
                }
            }
            
            if (!placed) 
                return;
            
            newState.Set(Age, value: 0);
        }
        else
        {
            newState.Set(Age, currentAge + 1);
        }
        
        message.World.SetBlock(new BlockInstance(newState), message.Position);
    }

    private TID GetActiveTexture(TID original, State state)
    {
        // todo: aspect with number of textures which is then used to determine the number of stages (subtract one because of dead stage)
        
        return original.Offset((Byte) (plant.GetStage(state) + 1 ?? 0));
    }
    
    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Int32? stage = plant.GetStage(state);
        
        return stage is null or 0
            ? new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.25f, z: 0.5f),
                new Vector3d(x: 0.175f, y: 0.25f, z: 0.175f))
            : new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.175f, y: 0.5f, z: 0.175f));
    }
}
