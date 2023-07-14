// <copyright file="UserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net.Control;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Input;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     A user interface that can be rendered to the screen.
/// </summary>
public abstract class UserInterface : IDisposable
{
    private static readonly Vector2i targetSize = new(x: 1920, y: 1080);
    private readonly bool drawBackground;
    private readonly InputListener inputListener;
    private readonly UIResources resources;

    /// <summary>
    ///     Creates a new user interface.
    /// </summary>
    /// <param name="inputListener">The input listener.</param>
    /// <param name="resources">The ui resources.</param>
    /// <param name="drawBackground">Whether to draw background of the ui.</param>
    protected UserInterface(InputListener inputListener, UIResources resources, bool drawBackground)
    {
        this.drawBackground = drawBackground;
        this.inputListener = inputListener;
        this.resources = resources;
    }

    internal Context Context { get; private set; } = null!;

    /// <summary>
    ///     The gui root control.
    /// </summary>
    public ControlBase Root => resources.GUI.Root;

    /// <summary>
    ///     Load the user interface.
    /// </summary>
    public void Load()
    {
        Root.ShouldDrawBackground = drawBackground;

        Context = new Context(inputListener, resources);

        SetSize(targetSize);
    }

    /// <summary>
    ///     Create the user interface controls.
    /// </summary>
    public void CreateControl()
    {
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
    public void Update()
    {
        resources.GUI.Update();
    }

    /// <summary>
    ///     Render the user interface.
    /// </summary>
    public void Render()
    {
        resources.GUI.Render();
    }

    /// <summary>
    ///     Resize the user interface.
    /// </summary>
    /// <param name="size">The new size.</param>
    public void Resize(Vector2i size)
    {
        SetSize(size);
    }

    private void SetSize(Vector2i size)
    {
        resources.GUI.Resize(size);

        float scale = Math.Min((float) size.X / targetSize.X, (float) size.Y / targetSize.Y);

        if (VMath.NearlyZero(scale)) return;

        resources.GUI.Root.Scale = scale;
    }

    #region IDisposable Support

    private bool disposed;

    /// <summary>
    ///     Override to dispose of resources.
    /// </summary>
    /// <param name="disposing">True if dispose was called by custom code.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Root.DeleteAllChildren();
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

    #endregion IDisposable Support
}
