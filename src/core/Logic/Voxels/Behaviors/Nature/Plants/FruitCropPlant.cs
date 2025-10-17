// <copyright file="FruitCropPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     A crop <see cref="Plant" /> that uses the <see cref="Foliage.LayoutType.Cross" /> layout places a fruit when fully
///     grown.
/// </summary>
public partial class FruitCropPlant : BlockBehavior, IBehavior<FruitCropPlant, BlockBehavior, Block>
{
    private const Int32 MaxAge = 3;
    private readonly GrowingPlant plant;

    private Block fruit = null!;

    private FruitCropPlant(Block subject) : base(subject)
    {
        plant = subject.Require<GrowingPlant>();
        plant.StageCount.Initializer.ContributeConstant(value: 2);

        var foliage = subject.Require<Foliage>();
        foliage.Layout.Initializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);
        foliage.Part.ContributeConstant(Foliage.PartType.Single);

        subject.Require<SingleTextured>().ActiveTexture.ContributeFunction(GetActiveTexture);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }

    [LateInitialization] private partial IAttribute<Int32> Age { get; set; }

    /// <summary>
    ///     The fruit block.
    /// </summary>
    public ResolvedProperty<CID?> Fruit { get; } = ResolvedProperty<CID?>.New<Exclusive<CID?, Void>>(nameof(Fruit));

    /// <inheritdoc />
    public static FruitCropPlant Construct(Block input)
    {
        return new FruitCropPlant(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<GrowingPlant.IMatureUpdateMessage>(OnMatureUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Fruit.Initialize(this);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Age = builder.Define(nameof(Age)).Int32(min: 0, MaxAge + 1).Attribute();
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Fruit.Get() == null)
            validator.ReportWarning("No fruit block is set");

        if (Fruit.Get() == Subject.ContentID)
            validator.ReportWarning("The fruit block cannot be the same as the growing block itself");

        fruit = Blocks.Instance.SafelyTranslateContentID(Fruit.Get());

        if (fruit == Blocks.Instance.Core.Error && Fruit.Get() != Blocks.Instance.Core.Error.ContentID)
            validator.ReportWarning($"The fruit block '{Fruit}' could not be found");
    }

    private void OnMatureUpdate(GrowingPlant.IMatureUpdateMessage message)
    {
        Int32 currentAge = message.State.Get(Age);

        State newState = message.State;

        if (currentAge == MaxAge)
        {
            var placed = false;

            if (message.Ground.SupportsFullGrowth.Get() && message.Ground.TryGrow(
                    message.World,
                    message.Position.Below(),
                    Voxels.Fluids.Instance.FreshWater,
                    FluidLevel.Two))
            {
                foreach (Orientation orientation in Orientations.ShuffledStart(message.Position))
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

        message.World.SetBlock(newState, message.Position);
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
