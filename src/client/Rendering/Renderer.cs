// <copyright file="Renderer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Abstract base class for renderers, which are used to render various things.
/// </summary>
public abstract class Renderer : IDisposable
{
    /// <summary>
    ///     Whether the renderer is enabled.
    /// </summary>
    public abstract bool IsEnabled { get; set; }

    private bool IsSetUp { get; set; }

    /// <summary>
    ///     Setup the renderer for active use.
    ///     Can only be enabled after the renderer has been setup.
    /// </summary>
    public void SetUp()
    {
        Debug.Assert(!disposed);

        Debug.Assert(!IsSetUp);
        IsSetUp = true;

        OnSetUp();
        IsEnabled = false;
    }

    /// <summary>
    ///     Use this to setup the renderer.
    /// </summary>
    protected abstract void OnSetUp();

    /// <summary>
    ///     Tear down the renderer.
    ///     Must be called before the renderer is disposed.
    ///     Can be setup again after calling this.
    /// </summary>
    public void TearDown()
    {
        Debug.Assert(!disposed);

        Debug.Assert(IsSetUp);
        IsSetUp = false;

        IsEnabled = false;
        OnTearDown();
    }

    /// <summary>
    ///     Use this to tear down the renderer.
    /// </summary>
    protected abstract void OnTearDown();

    /// <summary>
    ///     Update data for the renderer.
    ///     Call this every update cycle.
    /// </summary>
    public void Update()
    {
        Debug.Assert(!disposed);

        if (IsSetUp) OnUpdate();
    }

    /// <summary>
    ///     Use this to update the renderer.
    /// </summary>
    protected virtual void OnUpdate() {}

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        Debug.Assert(!IsSetUp);

        OnDispose(disposing);

        disposed = true;
    }

    /// <summary>
    ///     Called to dispose of resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose of managed resources.</param>
    protected virtual void OnDispose(bool disposing)
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Renderer()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
