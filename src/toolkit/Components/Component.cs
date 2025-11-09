// <copyright file="Component.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Toolkit.Components;

/// <summary>
///     Base class for components.
///     Components allow composition of functionality in a subject.
/// </summary>
/// <param name="subject">The subject that this component belongs to.</param>
/// <typeparam name="TSubject">The type of the subject.</typeparam>
public abstract class Component<TSubject>(TSubject subject) : IDisposable where TSubject : Composed
{
    /// <summary>
    ///     Get the subject of this component.
    /// </summary>
    public TSubject Subject { get; } = subject;
    
    /// <summary>
    /// Remove this component from its subject.
    /// </summary>
    public void RemoveSelf()
    {
        Subject.RemoveComponent(this);
    }

    #region DISPOSABLE

    /// <summary>
    ///     Override this method to dispose of resources.
    /// </summary>
    /// <param name="disposing">Whether the method is being called from Dispose or the finalizer.</param>
    protected virtual void Dispose(Boolean disposing) {}

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Component()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
