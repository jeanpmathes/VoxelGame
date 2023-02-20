// <copyright file="UserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using Gwen.Net.Control;
using Gwen.Net.OpenTk;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using VoxelGame.Core.Utilities;
using VoxelGame.Input;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     A user interface that can be rendered to the screen.
/// </summary>
public abstract class UserInterface : IDisposable
{
    private static readonly Vector2i targetSize = new(x: 1920, y: 1080);
    private readonly bool drawBackground;
    private readonly IGwenGui gui;
    private readonly InputListener inputListener;
    private readonly UIResources resources;

    /// <summary>
    ///     Creates a new user interface.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="inputListener">The input listener.</param>
    /// <param name="resources">The ui resources.</param>
    /// <param name="drawBackground">Whether to draw background of the ui.</param>
    protected UserInterface(GameWindow window, InputListener inputListener, UIResources resources, bool drawBackground)
    {
        gui = GwenGuiFactory.CreateFromGame(
            window,
            GwenGuiSettings.Default.From(
                settings =>
                {
                    settings.SkinFile = new FileInfo("VoxelSkin.png");
                    settings.DrawBackground = drawBackground;
                }));

        this.drawBackground = drawBackground;
        this.inputListener = inputListener;
        this.resources = resources;
    }

    internal Context Context { get; private set; } = null!;

    /// <summary>
    ///     The gui root control.
    /// </summary>
    public ControlBase Root => gui.Root;

    /// <summary>
    ///     Load the user interface.
    /// </summary>
    public void Load()
    {
        gui.Load();
        gui.Root.ShouldDrawBackground = drawBackground;

        Context = new Context(new FontHolder(gui.Root.Skin), inputListener, resources);

        SetSize(targetSize);
    }

    /// <summary>
    ///     Create the user interface controls.
    /// </summary>
    public abstract void CreateControl();

    /// <summary>
    ///     Render the user interface.
    /// </summary>
    public void Render()
    {
        GL.Disable(EnableCap.CullFace);

        gui.Render();

        GL.Enable(EnableCap.CullFace);
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
        gui.Resize(size);

        float scale = Math.Min((float) size.X / targetSize.X, (float) size.Y / targetSize.Y);

        if (VMath.NearlyZero(scale)) return;

        gui.Root.Scale = scale;
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
            gui.Dispose();
            Context.Dispose();
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
