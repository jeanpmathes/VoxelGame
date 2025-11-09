// <copyright file="SetOverlays.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Set whether to draw the block/fluid-in-head overlays.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetOverlays : Command
{
    /// <inheritdoc />
    public override String Name => "set-overlays";

    /// <inheritdoc />
    public override String HelpText => "Set whether the fluid/block overlays are enabled.";

    /// <exclude />
    public void Invoke(Boolean enabled)
    {
        Do(Context, enabled);
    }

    /// <summary>
    ///     Externally simulate a command invocation, setting the overlay state.
    /// </summary>
    public static void Do(Context context, Boolean enabled)
    {
        if (context.Player.GetComponent<OverlayDisplay>() is {} overlay)
            overlay.IsEnabled = enabled;
    }
}
