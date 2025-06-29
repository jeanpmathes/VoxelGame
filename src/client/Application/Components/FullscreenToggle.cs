// <copyright file="FullscreenToggle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.App;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Application.Components;

/// <summary>
///     Toggles the fullscreen mode of the client.
/// </summary>
public class FullscreenToggle : ApplicationComponent, IConstructible<Client, FullscreenToggle>
{
    private readonly Client client;

    private readonly ToggleButton button;

    /// <summary>
    ///     Create a new fullscreen toggle component for the client.
    /// </summary>
    public FullscreenToggle(Client client) : base(client)
    {
        this.client = client;

        button = client.Keybinds.GetToggle(client.Keybinds.Fullscreen);
    }

    /// <inheritdoc />
    public static FullscreenToggle Construct(Client input)
    {
        return new FullscreenToggle(input);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double delta, Timer? timer)
    {
        if (client.IsFocused && button.Changed) client.ToggleFullscreen();
    }
}
