// <copyright file="Densifying.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Allows inflow only if not above a certain height.
/// </summary>
public partial class Densifying : BlockBehavior, IBehavior<Densifying, BlockBehavior, Block>
{
    private readonly PartialHeight height;

    [Constructible]
    private Densifying(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);

        height = subject.Require<PartialHeight>();
    }

    private Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side _, Fluid _) = context;

        return height.GetHeight(state) < BlockHeight.Half;
    }
}
