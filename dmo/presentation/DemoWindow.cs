// <copyright file="DemoWindow.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Core;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Themes;
using VoxelGame.Logging;
using VoxelGame.Presentation.New.Platform;

namespace VoxelGame.Presentation.Demo;

internal class DemoWindow : Client
{
    private const Int32 MaxFrameSampleSize = 10000;

    private readonly GraphicalUserInterface gui;

    private readonly CircularTimeBuffer renderTimes;
    private readonly CircularTimeBuffer updateTimes;

    private DemoHarness? harness;

    private DemoWindow(WindowSettings windowSettings, Version version) : base(windowSettings, version)
    {
        gui = GraphicalUserInterface.Create(this, new Theme());

        updateTimes = new CircularTimeBuffer(MaxFrameSampleSize);
        renderTimes = new CircularTimeBuffer(MaxFrameSampleSize);

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(Object? sender, SizeChangeEventArgs e)
    {
        gui.Resize(Size);
    }

    protected override void OnInitialization(Timer? timer)
    {
        gui.Load(Size);

        harness = new DemoHarness();

        gui.Root?.Child = new ContentControl<DemoHarness>
        {
            Content = {Value = harness},
            ContentTemplate = {Value = ContentTemplate.Create<DemoHarness>(nameof(DemoHarnessView), DemoHarnessView.Create)}
        };

        gui.Root?.SetDebugOutlines(false);
    }

    protected override void OnLogicUpdate(Delta delta, Timer? timer)
    {
        updateTimes.Write(delta.RealTime);
        harness?.UpdateFrequency.SetValue(1 / updateTimes.Average);

        gui.Update();
    }

    protected override void OnRenderUpdate(Delta delta, Timer? timer)
    {
        renderTimes.Write(delta.RealTime);
        harness?.RenderFrequency.SetValue(1 / renderTimes.Average);

        gui.Render();
    }

    protected override void Dispose(Boolean disposing)
    {
        gui.Dispose();

        base.Dispose(disposing);
    }

    [STAThread]
    public static void Main(String[] args)
    {
        LoggingHelper.SetUpMockLogging();

        WindowSettings windowSettings = new()
        {
            Title = $"VoxelGame GUI Demo [{String.Join(" ", args)}]",
            Size = (960, 540),

            SupportPIX = args.Contains("--pix")
        };

        using DemoWindow window = new(windowSettings, new Version("0.0.0.1"));

        window.Run();
    }
}
