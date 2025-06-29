// <copyright file="Program.cs" company="VoxelGame">
//     This project serves a similar purpose as the Gwen.Net.Tests project, adapted for the VoxelGame rendering system.
//     It uses images from the Gwen.Net.Tests project, which are licensed under the MIT license.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net.Tests.Components;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Core;
using VoxelGame.Logging;
using VoxelGame.UI.Platform;
using Timer = VoxelGame.Core.Profiling.Timer;

namespace VoxelGame.UI.Tests;

internal class Program : Client
{
    private const Int32 MaxFrameSamples = 1000;
    private readonly IGwenGui gui;

    private readonly CircularTimeBuffer renderFrameTimes = new(MaxFrameSamples);
    private readonly CircularTimeBuffer updateFrameTimes = new(MaxFrameSamples);

    private UnitTestHarnessControls unitTestHarnessControls = null!;

    private Program(WindowSettings windowSettings, Version version) : base(windowSettings, version)
    {
        gui = GwenGuiFactory.CreateFromClient(this,
            GwenGuiSettings.Default.From(settings =>
            {
                settings.SkinFiles = [new FileInfo("DefaultSkin.png")];
                settings.ShaderFile = FileSystem.GetResourceDirectory("Shaders").GetFile("GUI.hlsl");
            }));

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(Object? sender, SizeChangeEventArgs e)
    {
        gui.Resize(Size);
    }

    [STAThread]
    internal static void Main()
    {
        LoggingHelper.SetUpMockLogging();

        WindowSettings windowSettings = new()
        {
            Title = "Gwen.net Unit Test",
            Size = new Vector2i(x: 800, y: 600)
        };

        using Client client = new Program(windowSettings, new Version("0.0.0.1"));
        client.Run();
    }

    protected override void OnInitialization(Timer? timer)
    {
        gui.Load();
        unitTestHarnessControls = new UnitTestHarnessControls(gui.Root);
    }

    protected override void OnLogicUpdate(Double delta, Timer? timer)
    {
        updateFrameTimes.Write(delta);
        unitTestHarnessControls.UpdateFps = 1 / updateFrameTimes.Average;

        gui.Update();
    }

    protected override void OnRenderUpdate(Double delta, Timer? timer)
    {
        renderFrameTimes.Write(delta);
        unitTestHarnessControls.RenderFps = 1 / renderFrameTimes.Average;

        gui.Render();
    }

    protected override void Dispose(Boolean disposing)
    {
        if (disposing)
        {
            SizeChanged -= OnSizeChanged;

            gui.Dispose();
        }

        base.Dispose(disposing);
    }
}
