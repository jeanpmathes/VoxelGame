// <copyright file="GwenGui.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Platform;
using Gwen.Net.Skin;
using OpenTK.Mathematics;
using VoxelGame.Support;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Input.Events;
using VoxelGame.UI.Platform.Input;
using VoxelGame.UI.Platform.Renderers;

namespace VoxelGame.UI.Platform;

internal class GwenGui : IGwenGui
{
    private Canvas canvas = null!;

    private bool disposed;
    private InputTranslator input = null!;
    private DirectXRendererBase rendererBase = null!;
    private SkinBase skin = null!;

    internal GwenGui(Client parent, GwenGuiSettings settings)
    {
        Parent = parent;
        Settings = settings;
    }

    internal GwenGuiSettings Settings { get; }

    internal Client Parent { get; }

    public ControlBase Root => canvas;

    public void Load()
    {
        GwenPlatform.Init(new VoxelGamePlatform(SetCursor));
        AttachToWindowEvents();
        rendererBase = new DirectXRenderer(Parent, Draw, Settings);

        skin = new TexturedBase(rendererBase, Settings.SkinFile, Settings.SkinLoadingErrorCallback)
        {
            DefaultFont = new Font(rendererBase, "Calibri", size: 11)
        };

        canvas = new Canvas(skin);
        input = new InputTranslator(canvas);

        canvas.SetSize(Parent.Size.X, Parent.Size.Y);
        rendererBase.Resize(Parent.Size.X, Parent.Size.Y);
        canvas.ShouldDrawBackground = true;
        canvas.BackgroundColor = skin.ModalBackground;
    }

    public void Render()
    {
        // This is not used, because the special Draw2D render hook is used.
    }

    public void Resize(Vector2i newSize)
    {
        rendererBase.Resize(newSize.X, newSize.Y);
        canvas.SetSize(newSize.X, newSize.Y);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal void Draw()
    {
        canvas.RenderCanvas();
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (!disposing) return;

        DetachWindowEvents();
        canvas.Dispose();
        skin.Dispose();
        rendererBase.Dispose();

        disposed = true;
    }

    ~GwenGui()
    {
        Dispose(disposing: false);
    }

    private void AttachToWindowEvents()
    {
        Parent.KeyUp += Parent_KeyUp;
        Parent.KeyDown += Parent_KeyDown;
        Parent.TextInput += Parent_TextInput;
        Parent.MouseButton += Parent_MouseButton;
        Parent.MouseMove += Parent_MouseMove;
        Parent.MouseWheel += Parent_MouseWheel;
    }

    private void DetachWindowEvents()
    {
        Parent.KeyUp -= Parent_KeyUp;
        Parent.KeyDown -= Parent_KeyDown;
        Parent.TextInput -= Parent_TextInput;
        Parent.MouseButton -= Parent_MouseButton;
        Parent.MouseMove -= Parent_MouseMove;
        Parent.MouseWheel -= Parent_MouseWheel;
    }

    private void Parent_KeyUp(object? sender, KeyboardKeyEventArgs obj)
    {
        input.ProcessKeyUp(obj);
    }

    private void Parent_KeyDown(object? sender, KeyboardKeyEventArgs obj)
    {
        input.ProcessKeyDown(obj);
    }

    private void Parent_TextInput(object? sender, TextInputEventArgs obj)
    {
        input.ProcessTextInput(obj);
    }

    private void Parent_MouseButton(object? sender, MouseButtonEventArgs obj)
    {
        input.ProcessMouseButton(obj);
    }

    private void Parent_MouseMove(object? sender, MouseMoveEventArgs obj)
    {
        input.ProcessMouseMove(obj);
    }

    private void Parent_MouseWheel(object? sender, MouseWheelEventArgs obj)
    {
        input.ProcessMouseWheel(obj);
    }

    private void SetCursor(MouseCursor mouseCursor)
    {
        Parent.Cursor = mouseCursor;
    }
}
