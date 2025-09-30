// <copyright file="BlockBehavior.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     Base class for behaviors that apply to blocks.
/// </summary>
public class BlockBehavior : Behavior<BlockBehavior, Block>
{
    /// <summary>
    ///     Creates a new instance of the <see cref="BlockBehavior" /> class.
    ///     Behaviors should be created through the methods on the subject, not directly.
    /// </summary>
    /// <param name="subject">The subject that this behavior applies to.</param>
    protected BlockBehavior(Block subject) : base(subject) {}

    /// <summary>
    ///     Override this method to perform work during block initialization.
    /// </summary>
    /// <param name="properties">
    ///     The properties that define the block's aspects.
    ///     Can be contributed to by the behaviors.
    /// </param>
    public virtual void OnInitialize(BlockProperties properties) {}

    /// <summary>
    ///     Override this method to define the states of the behavior.
    /// </summary>
    /// <param name="builder">The builder to create attributes with.</param>
    public virtual void DefineState(IStateBuilder builder) {}
}
