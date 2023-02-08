// <copyright file="SetOverlays.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Set whether to draw the block/fluid-in-head overlays.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetOverlays : Command
{
    /// <inheritdoc />
    public override string Name => "set-overlays";

    /// <inheritdoc />
    public override string HelpText => "Set whether the fluid/block overlays are enabled.";

    /// <exclude />
    public void Invoke(bool enabled)
    {
        Do(Context, enabled);
    }

    /// <summary>
    ///     Externally simulate a command invocation, setting the overlay state.
    /// </summary>
    public static void Do(Context context, bool enabled)
    {
        context.Player.OverlayEnabled = enabled;
    }
}

