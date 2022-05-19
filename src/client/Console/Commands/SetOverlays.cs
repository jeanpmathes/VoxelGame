// <copyright file="Clear.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
        Context.Player.OverlayEnabled = enabled;
    }
}
