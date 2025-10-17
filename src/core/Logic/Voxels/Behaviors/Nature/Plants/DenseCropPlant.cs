// <copyright file="DenseCropPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     A <see cref="Plant" /> that uses the <see cref="Foliage.LayoutType.DenseCrop" /> layout.
/// </summary>
public class DenseCropPlant : BlockBehavior, IBehavior<DenseCropPlant, BlockBehavior, Block>
{
    private const Int32 StageCount = 5;
    
    private readonly GrowingPlant plant;

    private DenseCropPlant(Block subject) : base(subject)
    {
        plant = subject.Require<GrowingPlant>();
        plant.StageCount.Initializer.ContributeConstant(StageCount);
        plant.FirstFullStage.Initializer.ContributeConstant(value: 3);

        subject.Require<Foliage>().Layout.Initializer.ContributeConstant(Foliage.LayoutType.DenseCrop, exclusive: true);
        subject.Require<SingleTextured>().ActiveTexture.ContributeFunction(GetActiveTexture);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }
    
    /// <inheritdoc />
    public static DenseCropPlant Construct(Block input)
    {
        return new DenseCropPlant(input);
    }

    private TID GetActiveTexture(TID original, State state)
    {
        return original.Offset((Byte) (plant.GetStage(state) ?? StageCount));
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Int32? currentStage = plant.GetStage(state);

        if (currentStage is {} aliveStage)
        {
            return aliveStage switch
            {
                0 => BoundingVolume.BlockWithHeight(height: 3),
                1 => BoundingVolume.BlockWithHeight(height: 6),
                2 => BoundingVolume.BlockWithHeight(height: 10),
                3 => BoundingVolume.BlockWithHeight(height: 12),
                4 => BoundingVolume.BlockWithHeight(height: 15),
                _ => throw Exceptions.UnsupportedValue(aliveStage)
            };
        }

        return BoundingVolume.BlockWithHeight(height: 3);
    }
}
