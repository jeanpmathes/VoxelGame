// <copyright file="DoubleCropPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     A <see cref="Plant" /> that uses the <see cref="Foliage.LayoutType.Crop" /> layout and has double stages.
/// </summary>
public partial class DoubleCropPlant : BlockBehavior, IBehavior<DoubleCropPlant, BlockBehavior, Block>
{
    private const Int32 StageCount = 5;
    
    /// <summary>
    /// The first full stage is actually the one before reaching double height.
    /// This is necessary to prevent reaching double height on soil that cannot support it.
    /// </summary>
    private const Int32 FirstFullStage = 1;
    
    private readonly Composite composite;
    private readonly GrowingPlant plant;

    [Constructible]
    private DoubleCropPlant(Block subject) : base(subject)
    {
        plant = subject.Require<GrowingPlant>();
        plant.StageCount.Initializer.ContributeConstant(StageCount);
        plant.FirstFullStage.Initializer.ContributeConstant(FirstFullStage);
        plant.CanGrow.ContributeFunction(GetCanGrow);

        composite = subject.Require<Composite>();
        composite.MaximumSize.Initializer.ContributeConstant((1, 2, 1));
        composite.Size.ContributeFunction(GetSize);

        var foliage = subject.Require<Foliage>();
        foliage.Layout.Initializer.ContributeConstant(Foliage.LayoutType.Crop, exclusive: true);
        foliage.Part.ContributeFunction(GetPart);

        subject.Require<VerticalTextureSelector>().HorizontalOffset.ContributeFunction(GetHorizontalOffset);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }

    private Boolean GetCanGrow(Boolean original, State state)
    {
        return composite.GetPartPosition(state).Y == 0;
    }

    private Vector3i GetSize(Vector3i original, State state)
    {
        return IsDouble(state) ? (1, 2, 1) : (1, 1, 1);
    }

    private Foliage.PartType GetPart(Foliage.PartType original, State state)
    {
        if (!IsDouble(state))
            return Foliage.PartType.Single;

        return composite.GetPartPosition(state).Y == 0 ? Foliage.PartType.DoubleLower : Foliage.PartType.DoubleUpper;
    }
    
    private Int32 GetHorizontalOffset(Int32 original, State state)
    {
        return plant.GetStage(state) + 1 ?? 0;
    }

    private Boolean IsDouble(State state)
    {
        return plant.GetStage(state) > FirstFullStage;
    }
    
    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Int32? currentStage = plant.GetStage(state);
        Boolean isLower = composite.GetPartPosition(state).Y == 0;

        if (currentStage is not {} aliveStage)
            return BoundingVolume.BlockWithHeight(height: 15);

        Boolean isLowerAndStillGrowing = isLower && aliveStage == 0;
        Boolean isUpperAndStillGrowing = !isLower && aliveStage == 2;

        if (isLowerAndStillGrowing || isUpperAndStillGrowing)
            return BoundingVolume.BlockWithHeight(height: 7);

        return BoundingVolume.BlockWithHeight(height: 15);
    }
}
