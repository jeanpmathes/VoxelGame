// <copyright file="Program.cs" company="VoxelGame">
//     This project serves a similar purpose as the Gwen.Net.Tests project, adapted for the VoxelGame rendering system.
//     It uses images from the Gwen.Net.Tests project, which are licensed under the MIT license.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using Gwen.Net.Tests.Components;
using OpenTK.Mathematics;
using VoxelGame.Core;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.UI.Platform;

[assembly: ComVisible(visibility: false)]

namespace VoxelGame.UI.Tests;

internal class Program : Client
{
    private const int MaxFrameSamples = 1000;
    private readonly IGwenGui gui;
    private readonly CircularTimeBuffer renderFrameTimes = new(MaxFrameSamples);
    private readonly CircularTimeBuffer updateFrameTimes = new(MaxFrameSamples);

    private UnitTestHarnessControls unitTestHarnessControls = null!;

    private Program(WindowSettings windowSettings) : base(windowSettings)
    {
        gui = GwenGuiFactory.CreateFromClient(this,
            GwenGuiSettings.Default.From(settings =>
            {
                settings.SkinFiles = new[] {new FileInfo("DefaultSkin.png")};
                settings.ShaderFile = FileSystem.GetResourceDirectory("Shaders").GetFile("GUI.hlsl");
            }));

        OnSizeChange += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangeEventArgs e)
    {
        gui.Resize(Size);
    }

    [STAThread]
    internal static void Main()
    {
        LoggingHelper.SetupMockLogging();
        ApplicationInformation.Initialize("0.0.0.1");

        WindowSettings windowSettings = new()
        {
            Title = "Gwen.net Unit Test",
            Size = new Vector2i(x: 800, y: 600)
        };

        using Client client = new Program(windowSettings);
        client.Run();
    }

    protected override void OnInit()
    {
        gui.Load();
        unitTestHarnessControls = new UnitTestHarnessControls(gui.Root);
    }

    protected override void OnUpdate(double delta)
    {
        updateFrameTimes.Write(delta);
        unitTestHarnessControls.UpdateFps = 1 / updateFrameTimes.Average;

        gui.Update();
    }

    protected override void OnRender(double delta)
    {
        renderFrameTimes.Write(delta);
        unitTestHarnessControls.RenderFps = 1 / renderFrameTimes.Average;

        gui.Render();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            OnSizeChange -= OnSizeChanged;

            gui.Dispose();
        }

        base.Dispose(disposing);
    }
}
