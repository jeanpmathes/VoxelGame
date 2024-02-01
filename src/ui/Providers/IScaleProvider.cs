// <copyright file="IScaleProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides the scale of the UI.
/// </summary>
public interface IScaleProvider
{
    /// <summary>
    ///     Get the scale of the UI.
    /// </summary>
    public float Scale { get; }

    /// <summary>
    ///     Subscribe to changes of the scale of the UI.
    /// </summary>
    /// <param name="action">The action to be called when the scale changes.</param>
    /// <returns>An <see cref="IDisposable" /> that can be used to unsubscribe.</returns>
    public IDisposable Subscribe(Action<float> action);
}
