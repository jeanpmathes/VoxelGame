// <copyright file="SetDevmode.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Enable or disable the devmode, which allows flying.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetDevmode : Command
{
    /// <inheritdoc />
    public override string Name => "set-devmode";

    /// <inheritdoc />
    public override string HelpText => "Enable or disable the devmode.";

    /// <exclude />
    public void Invoke(bool enabled)
    {
        SetPhysics.Do(Context, !enabled);
        SetOverlays.Do(Context, !enabled);
    }
}
