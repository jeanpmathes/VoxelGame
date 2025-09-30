// <copyright file="Loose.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Materials;

/// <summary>
///     A block made from loose materials, creating a permeable surface.
/// </summary>
public class Loose : BlockBehavior, IBehavior<Loose, BlockBehavior, Block>
{
    private Loose(Block subject) : base(subject)
    {
        subject.Require<Membrane>().MaxViscosityInitializer.ContributeConstant(value: 100);
        subject.Require<Fillable>().IsFluidRenderedInitializer.ContributeConstant(value: false);
    }

    /// <inheritdoc />
    public static Loose Construct(Block input)
    {
        return new Loose(input);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!Subject.Is<Wet>())
        {
            validator.ReportWarning("Loose blocks must be able to get wet in some way, preferably with visual representation of that");
        }
    }
}
