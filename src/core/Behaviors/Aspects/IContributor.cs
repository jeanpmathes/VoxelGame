// <copyright file="IContributor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TContext"></typeparam>
public interface IContributor<TValue, in TContext>
{
    /// <summary>
    ///     Contributes to the original value based on the context provided.
    /// </summary>
    /// <param name="original">The original value to contribute to.</param>
    /// <param name="context">The context in which the value is being evaluated.</param>
    /// <returns>The value with the contribution applied.</returns>
    TValue Contribute(TValue original, TContext context);
}
