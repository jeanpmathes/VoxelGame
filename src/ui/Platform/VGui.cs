﻿// <copyright file="VGui.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Platform;
using Gwen.Net.Skin;
using OpenTK.Mathematics;
using VoxelGame.Support.Core;
using VoxelGame.Support.Input.Events;
using VoxelGame.UI.Platform.Input;
using VoxelGame.UI.Platform.Renderer;

namespace VoxelGame.UI.Platform;

internal sealed class VGui : IGwenGui
{
    private readonly List<Action> inputEvents = new();
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
        GwenPlatform.Init(new VoxelGamePlatform(Parent.Mouse.SetCursorType));
        AttachToWindowEvents();

        try
        {
            renderer = new DirectXRenderer(Parent, Settings);
        }
        catch (InvalidOperationException)
        {
            return;
        }

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

    public void Update()
    {
        foreach (Action inputEvent in inputEvents) inputEvent();

        inputEvents.Clear();
    }

    public void Render()
    {
        canvas.RenderCanvas();

        // Helps the UI to recognize that the mouse is over a control if that control was just added:
        input.ProcessMouseMove(new MouseMoveEventArgs
        {
            Position = Parent.Mouse.Position.ToVector2()
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
        Parent.KeyUp += OnKeyUp;
        Parent.KeyDown += OnKeyDown;
        Parent.TextInput += OnTextInput;
        Parent.MouseButton += OnMouseButton;
        Parent.MouseMove += OnMouseMove;
        Parent.MouseWheel += OnMouseWheel;
    }

    private void DetachWindowEvents()
    {
        Parent.KeyUp -= OnKeyUp;
        Parent.KeyDown -= OnKeyDown;
        Parent.TextInput -= OnTextInput;
        Parent.MouseButton -= OnMouseButton;
        Parent.MouseMove -= OnMouseMove;
        Parent.MouseWheel -= OnMouseWheel;
    }

    private void OnKeyUp(object? sender, KeyboardKeyEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessKeyUp(obj));
    }

    private void OnKeyDown(object? sender, KeyboardKeyEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessKeyDown(obj));
    }

    private void OnTextInput(object? sender, TextInputEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessTextInput(obj));
    }

    private void OnMouseButton(object? sender, MouseButtonEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessMouseButton(obj));
    }

    private void OnMouseMove(object? sender, MouseMoveEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessMouseMove(obj));
    }

    private void OnMouseWheel(object? sender, MouseWheelEventArgs obj)
    {
        inputEvents.Add(() => input.ProcessMouseWheel(obj));
    }
}
