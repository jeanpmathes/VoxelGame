// <copyright file="VGui.cs" company="Gwen.Net">
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

namespace VoxelGame.UI.Platform;

internal sealed class VGui : IGwenGui
{
    private Canvas canvas = null!;

    private bool disposed;
    private InputTranslator input = null!;
    private DirectXRenderer renderer = null!;
    private SkinBase skin = null!;

    internal VGui(Client parent, GwenGuiSettings settings)
    {
        Parent = parent;
        Settings = settings;
    }

    private GwenGuiSettings Settings { get; }

    private Client Parent { get; }

    public ControlBase Root => canvas;

    public void Load()
    {
        GwenPlatform.Init(new VoxelGamePlatform(SetCursor));
        AttachToWindowEvents();
        renderer = new DirectXRenderer(Parent, Draw, Settings);

        skin = new TexturedBase(renderer, Settings.SkinFile, Settings.SkinLoadingErrorCallback)
        {
            DefaultFont = new Font(renderer, "Calibri", size: 11)
        };

        canvas = new Canvas(skin);
        input = new InputTranslator(canvas);

        canvas.SetSize(Parent.Size.X, Parent.Size.Y);
        canvas.ShouldDrawBackground = true;
        canvas.BackgroundColor = skin.ModalBackground;

        renderer.FinishLoading();
    }

    public void Render()
    {
        // This is not used, because the special Draw2D render hook is used.
    }

    public void Resize(Vector2i newSize)
    {
        canvas.SetSize(newSize.X, newSize.Y);
        renderer.Resize(newSize.ToVector2());
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Draw()
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
        renderer.Dispose();

        disposed = true;
    }

    ~VGui()
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
