// <copyright file="UserInterface.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net.Control;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Input;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     A user interface that can be rendered to the screen.
/// </summary>
public abstract class UserInterface : IDisposable
{
    private static readonly Vector2 targetSize = new(x: 1920, y: 1080);

    private readonly Input input;
    private readonly IScaleProvider scale;
    private readonly UserInterfaceResources resources;
    private readonly Boolean drawBackground;

    private readonly IDisposable scaleSubscription;

    private Vector2i currentSize = Vector2i.Zero;

    /// <summary>
    ///     Creates a new user interface.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="scale">Provides the scale of the ui.</param>
    /// <param name="resources">The ui resources.</param>
    /// <param name="drawBackground">Whether to draw background of the ui.</param>
    protected UserInterface(Input input, IScaleProvider scale, UserInterfaceResources resources, Boolean drawBackground)
    {
        this.input = input;
        this.scale = scale;
        this.resources = resources;
        this.drawBackground = drawBackground;

        scaleSubscription = scale.Subscribe(_ => UpdateScale());
    }

    internal Context Context { get; private set; } = null!;

    /// <summary>
    ///     The gui root control.
    /// </summary>
    public ControlBase Root => resources.GUI.Root;

    /// <summary>
    ///     Whether it is safe to close this user interface (e.g. when the application is closing).
    /// </summary>
    public Boolean IsSafeToClose => !Context.IsInModal;

    /// <summary>
    ///     Load the user interface.
    /// </summary>
    public void Load()
    {
        Throw.IfDisposed(disposed);

        Root.ShouldDrawBackground = drawBackground;

        Context = new Context(input, resources);
    }

    /// <summary>
    ///     Create the user interface controls.
    /// </summary>
    public void CreateControl()
    {
        Throw.IfDisposed(disposed);

        Root.DeleteAllChildren();
        CreateNewControl();
    }

    /// <summary>
    ///     Create the new control.
    /// </summary>
    protected abstract void CreateNewControl();

    /// <summary>
    ///     Update the user interface. This handles the input.
    /// </summary>
    public void LogicUpdate()
    {
        Throw.IfDisposed(disposed);

        resources.GUI.Update();
    }

    /// <summary>
    ///     Render the user interface.
    /// </summary>
    public void RenderUpdate()
    {
        Throw.IfDisposed(disposed);

        resources.GUI.Render();
    }

    /// <summary>
    ///     Resize the user interface.
    /// </summary>
    /// <param name="size">The new size.</param>
    public void Resize(Vector2i size)
    {
        Throw.IfDisposed(disposed);

        SetSize(size);
    }

    private void SetSize(Vector2i size)
    {
        resources.GUI.Resize(size);

        currentSize = size;
        UpdateScale();
    }

    private void UpdateScale()
    {
        Single newScale = (currentSize.ToVector2() / targetSize).MinComponent() * scale.Scale;

        if (VMath.NearlyZero(newScale)) return;

        resources.GUI.Root.Scale = newScale;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Override to dispose of resources.
    /// </summary>
    /// <param name="disposing">True if dispose was called by custom code.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Root.DeleteAllChildren();

            scaleSubscription.Dispose();
        }

        disposed = true;
    }

    /// <summary>
    ///     Dispose of resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion DISPOSABLE
}
