// <copyright file="Program.cs" company="VoxelGame">
//     This project serves a similar purpose as the Gwen.Net.Tests project, adapted for the VoxelGame rendering system.
//     It uses images from the Gwen.Net.Tests project, which are licensed under the MIT license.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net.Tests.Components;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Logging;
using VoxelGame.Support;
using VoxelGame.UI.Platform;

namespace VoxelGame.UI.Tests;

internal class Program : Support.Client
{
    private const int MaxFrameSamples = 1000;
    private readonly IGwenGui gui;
    private readonly CircularTimeBuffer renderFrameTimes = new(MaxFrameSamples);
    private readonly CircularTimeBuffer updateFrameTimes = new(MaxFrameSamples);

    private UnitTestHarnessControls unitTestHarnessControls = null!;

    private Program(WindowSettings windowSettings) : base(windowSettings, enableSpace: false)
    {
        gui = GwenGuiFactory.CreateFromClient(this, GwenGuiSettings.Default.From(settings => settings.SkinFile = new FileInfo("DefaultSkin2.png")));
    }

    [STAThread]
    internal static void Main()
    {
        LoggingHelper.SetupMockLogging();

        WindowSettings windowSettings = new()
        {
            Title = "Gwen.net Unit Test",
            Size = new Vector2i(x: 800, y: 600)
        };

        using Support.Client client = new Program(windowSettings);
        client.Run();
    }

    protected override void OnInit()
    {
        gui.Load();
        unitTestHarnessControls = new UnitTestHarnessControls(gui.Root);
    }

    protected override void OnResize(Vector2i size)
    {
        gui.Resize(size);
    }

    protected override void OnUpdate(double delta)
    {
        updateFrameTimes.Write(delta);
        unitTestHarnessControls.UpdateFps = 1 / updateFrameTimes.Average;
    }

    protected override void OnRender(double delta)
    {
        renderFrameTimes.Write(delta);
        unitTestHarnessControls.RenderFps = 1 / renderFrameTimes.Average;

        gui.Render();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) gui.Dispose();

        base.Dispose(disposing);
    }
}
