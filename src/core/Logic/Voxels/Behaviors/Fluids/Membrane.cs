// <copyright file="Membrane.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Controls inflow into the block, allowing to filter out which fluids can pass through.
/// </summary>
public partial class Membrane : BlockBehavior, IBehavior<Membrane, BlockBehavior, Block>
{
    [Constructible]
    private Membrane(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
    }

    /// <summary>
    ///     Only fluids with a viscosity less than this value can flow into the block.
    /// </summary>
    public ResolvedProperty<Int32> MaxViscosity { get; } = ResolvedProperty<Int32>.New<Minimum<Int32, Void>>(nameof(MaxViscosity), initial: 1000);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        MaxViscosity.Initialize(this);
    }

    private Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State _, Side _, Fluid fluid) = context;

        return fluid.Viscosity < MaxViscosity.Get();
    }
}
