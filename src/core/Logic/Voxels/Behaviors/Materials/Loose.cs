// <copyright file="Loose.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     A block made from loose materials, creating a permeable surface.
/// </summary>
public partial class Loose : BlockBehavior, IBehavior<Loose, BlockBehavior, Block>
{
    [Constructible]
    private Loose(Block subject) : base(subject)
    {
        subject.Require<Membrane>().MaxViscosity.Initializer.ContributeConstant(value: 100);
        subject.Require<Fillable>().IsFluidMeshed.Initializer.ContributeConstant(value: false);
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
