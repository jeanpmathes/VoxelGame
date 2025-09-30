// <copyright file="ScreenshotController.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Controls the screenshot functionality in a <see cref="SessionScene" />.
/// </summary>
public class ScreenshotController : SceneComponent, IConstructible<SessionScene, ScreenshotController>
{
    private readonly PushButton button;
    private readonly SessionScene scene;

    private ScreenshotController(SessionScene scene) : base(scene)
    {
        this.scene = scene;

        button = scene.Client.Keybinds.GetPushButton(scene.Client.Keybinds.Screenshot);
    }

    /// <inheritdoc />
    public static ScreenshotController Construct(SessionScene input)
    {
        return new ScreenshotController(input);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        if (scene.CanHandleGameInput && button.Pushed)
        {
            scene.Client.TakeScreenshot(Program.ScreenshotDirectory);
        }
    }
}
