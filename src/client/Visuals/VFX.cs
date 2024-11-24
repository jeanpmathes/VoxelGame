// <copyright file="VFX.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals;

#pragma warning disable S101 // Naming.

/// <summary>
///     Abstract base class for VFXs, which are used to display various things.
/// </summary>
public abstract class VFX : IDisposable
{
    /// <summary>
    ///     Whether the VFX is enabled.
    /// </summary>
    public abstract Boolean IsEnabled { get; set; }

    private Boolean IsSetUp { get; set; }

    /// <summary>
    ///     Set up the VFX for active use.
    ///     Can only be enabled after the VFX has been setup.
    /// </summary>
    public void SetUp()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(!IsSetUp);
        IsSetUp = true;

        OnSetUp();
        IsEnabled = false;
    }

    /// <summary>
    ///     Use this to setup the VFX.
    /// </summary>
    protected abstract void OnSetUp();

    /// <summary>
    ///     Tear down the VFX.
    ///     Must be called before the VFX is disposed.
    ///     Can be setup again after calling this.
    /// </summary>
    public void TearDown()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(IsSetUp);
        IsSetUp = false;

        IsEnabled = false;
        OnTearDown();
    }

    /// <summary>
    ///     Use this to tear down the VFX.
    /// </summary>
    protected abstract void OnTearDown();

    /// <summary>
    ///     Update data for the VFX.
    ///     Call this every update cycle.
    /// </summary>
    public void Update()
    {
        Throw.IfDisposed(disposed);

        if (IsSetUp) OnUpdate();
    }

    /// <summary>
    ///     Use this to update the VFX.
    /// </summary>
    protected virtual void OnUpdate() {}

    #region IDisposable Support

    private Boolean disposed;

    /// <summary>
    ///     Override to determine the disposing behavior.
    /// </summary>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        Debug.Assert(!IsSetUp);

        disposed = true;
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
    ~VFX()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
