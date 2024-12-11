// <copyright file="Actor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Actors;

/// <summary>
///     An actor is anything that can be added to a world, but is not the world itself.
/// </summary>
public abstract class Actor : IDisposable
{
    /// <summary>
    ///     Gets the world in which this actor is located.
    ///     Using an actor without a world is not valid.
    /// </summary>
    public World World { get; private set; } = null!;

    /// <summary>
    ///     Called when this actor is added to a world.
    /// </summary>
    /// <param name="world">The world to which this actor was added.</param>
    public void OnAdd(World world)
    {
        World = world;

        AddedToWorld?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Invoked when this actor is added to a world.
    /// </summary>
    protected event EventHandler? AddedToWorld;

    /// <summary>
    ///     Called when this actor is removed from a world.
    /// </summary>
    public void OnRemove()
    {
        RemovedFromWorld?.Invoke(this, EventArgs.Empty);

        World = null!;
    }

    /// <summary>
    ///     Invoked when this actor is removed from a world.
    /// </summary>
    protected event EventHandler? RemovedFromWorld;

    /// <summary>
    ///     Tick this actor. An actor is ticked every update.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public virtual void Tick(Double deltaTime) {}

    #region IDisposable Support

    private Boolean disposed;

    /// <summary>
    ///     Disposes this actor.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Actor()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Disposes this actor.
    /// </summary>
    /// <param name="disposing">True if called by code.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        disposed = true;
    }

    #endregion IDisposable Support
}
