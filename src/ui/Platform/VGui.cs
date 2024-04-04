// <copyright file="VGui.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Platform;
using Gwen.Net.Skin;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Core;
using VoxelGame.Support.Input.Events;
using VoxelGame.UI.Platform.Input;
using VoxelGame.UI.Platform.Renderer;

namespace VoxelGame.UI.Platform;

internal sealed class VGui : IGwenGui
{
    private readonly List<SkinBase> skins = new();
    private List<Action> inputEvents = new();
    private Canvas canvas = null!;

    private InputTranslator input = null!;
    private DirectXRenderer renderer = null!;

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
        Throw.IfDisposed(disposed);

        GwenPlatform.Init(new VoxelGamePlatform(Parent.Input.Mouse.SetCursorType));
        AttachToWindowEvents();

        try
        {
            renderer = new DirectXRenderer(Parent, Settings);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        Debug.Assert(Settings.SkinFiles.Any());

        foreach ((FileInfo skinFile, Int32 index) in Settings.SkinFiles.Select((f, i) => (f, i)))
        {
            var failed = false;

            SkinBase skin = new TexturedBase(renderer,
                skinFile,
                e =>
                {
                    Settings.SkinLoadingErrorCallback(skinFile, e);
                    failed = true;
                })
            {
                DefaultFont = new Font(renderer, "Calibri", size: 11)
            };

            skins.Add(skin);

            if (!failed) Settings.SkinLoadedCallback(index, skin);
        }

        canvas = new Canvas(skins[index: 0]);
        input = new InputTranslator(canvas);

        renderer.Resize(Parent.Size);

        canvas.SetSize(Parent.Size.X, Parent.Size.Y);
        canvas.ShouldDrawBackground = true;
        canvas.BackgroundColor = skins[index: 0].ModalBackground;

        renderer.FinishLoading();
    }

    public void Update()
    {
        Throw.IfDisposed(disposed);

        // While handling an event, code might be executed that passes control to the OS.
        // As such, new events might be invoked, causing problems with the iteration.

        List<Action> events = new();
        VMath.Swap(ref events, ref inputEvents);

        foreach (Action inputEvent in events) inputEvent();
    }

    public void Render()
    {
        Throw.IfDisposed(disposed);

        canvas.RenderCanvas();

        // Helps the UI to recognize that the mouse is over a control if that control was just added:
        input.ProcessMouseMove(new MouseMoveEventArgs
        {
            Position = Parent.Input.Mouse.Position.ToVector2()
        });
    }

    public void Resize(Vector2i newSize)
    {
        renderer.Resize(newSize.ToVector2());
        canvas.SetSize(newSize.X, newSize.Y);
    }

    private void AttachToWindowEvents()
    {
        Parent.Input.KeyUp += OnKeyUp;
        Parent.Input.KeyDown += OnKeyDown;
        Parent.Input.TextInput += OnTextInput;
        Parent.Input.MouseButton += OnMouseButton;
        Parent.Input.MouseMove += OnMouseMove;
        Parent.Input.MouseWheel += OnMouseWheel;
    }

    private void DetachWindowEvents()
    {
        Parent.Input.KeyUp -= OnKeyUp;
        Parent.Input.KeyDown -= OnKeyDown;
        Parent.Input.TextInput -= OnTextInput;
        Parent.Input.MouseButton -= OnMouseButton;
        Parent.Input.MouseMove -= OnMouseMove;
        Parent.Input.MouseWheel -= OnMouseWheel;
    }

    private void OnKeyUp(Object? sender, KeyboardKeyEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessKeyUp(obj));
    }

    private void OnKeyDown(Object? sender, KeyboardKeyEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessKeyDown(obj));
    }

    private void OnTextInput(Object? sender, TextInputEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessTextInput(obj));
    }

    private void OnMouseButton(Object? sender, MouseButtonEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessMouseButton(obj));
    }

    private void OnMouseMove(Object? sender, MouseMoveEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessMouseMove(obj));
    }

    private void OnMouseWheel(Object? sender, MouseWheelEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessMouseWheel(obj));
    }

    #region IDisposable Support

    private Boolean disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (!disposing) return;

        DetachWindowEvents();

        canvas.Dispose();

        foreach (SkinBase skin in skins) skin.Dispose();

        renderer.Dispose();

        disposed = true;
    }

    ~VGui()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
