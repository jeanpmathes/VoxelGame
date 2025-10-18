// <copyright file="FullscreenToggle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.App;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Application.Components;

/// <summary>
///     Toggles the fullscreen mode of the client.
/// </summary>
public partial class FullscreenToggle : ApplicationComponent
{
    private readonly ToggleButton button;
    private readonly Client client;

    [Constructible]
    private FullscreenToggle(Client client) : base(client)
    {
        this.client = client;

        button = client.Keybinds.GetToggle(client.Keybinds.Fullscreen);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double delta, Timer? timer)
    {
        if (client.IsFocused && button.Changed) client.ToggleFullscreen();
    }
}
