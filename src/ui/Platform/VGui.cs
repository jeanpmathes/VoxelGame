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
using VoxelGame.Support.Input.Events;
using VoxelGame.UI.Platform.Input;
using VoxelGame.UI.Platform.Renderer;

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
        GwenPlatform.Init(new VoxelGamePlatform(Parent.SetCursor));
        AttachToWindowEvents();
        renderer = new DirectXRenderer(Parent, Settings);

        skin = new TexturedBase(renderer, Settings.SkinFile, Settings.SkinLoadingErrorCallback)
        {
            DefaultFont = new Font(renderer, "Calibri", size: 11)
        };

        canvas = new Canvas(skin);
        input = new InputTranslator(canvas);

        renderer.Resize(Parent.Size);

        canvas.SetSize(Parent.Size.X, Parent.Size.Y);
        canvas.ShouldDrawBackground = true;
        canvas.BackgroundColor = skin.ModalBackground;

        renderer.FinishLoading();
    }

    public void Render()
    {
        canvas.RenderCanvas();

        // Helps the UI to recognize that the mouse is over a control if that control was just added:
        input.ProcessMouseMove(new MouseMoveEventArgs
        {
            Position = Parent.MousePosition.ToVector2()
        });
    }

    public void Resize(Vector2i newSize)
    {
        renderer.Resize(newSize.ToVector2());
        canvas.SetSize(newSize.X, newSize.Y);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
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
}
