// <copyright file="Pipe.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Guides the flow of fluids.
/// </summary>
public class Pipe : BlockBehavior, IBehavior<Pipe, BlockBehavior, Block>
{
    private Pipe(Block subject) : base(subject)
    {
        subject.Require<Piped>();

        var fillable = subject.Require<Fillable>();
        fillable.IsFluidMeshed.Initializer.ContributeConstant(value: false);
        fillable.IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
        fillable.IsOutflowAllowed.ContributeFunction(GetIsOutflowAllowed);

        OpenSides = Aspect<Sides, State>.New<Exclusive<Sides, State>>(nameof(OpenSides), this);
    }

    /// <summary>
    ///     Get the sides which are open in a given state.
    /// </summary>
    public Aspect<Sides, State> OpenSides { get; }

    /// <inheritdoc />
    public static Pipe Construct(Block input)
    {
        return new Pipe(input);
    }

    private Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side side, Fluid _) = context;

        return OpenSides.GetValue(Sides.None, state).HasFlag(side.ToFlag());
    }

    private Boolean GetIsOutflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side side, Fluid _) = context;

        return OpenSides.GetValue(Sides.None, state).HasFlag(side.ToFlag());
    }
}
