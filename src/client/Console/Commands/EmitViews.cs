// <copyright file="EmitViews.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Emit views of the generated data for debugging.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EmitViews : Command
{
    /// <inheritdoc />
    public override string Name => "emit-views";

    /// <inheritdoc />
    public override string HelpText => "Emit views of the generated map for debugging.";

    /// <exclude />
    public void Invoke()
    {
        Context.Player.World.EmitViews();
    }
}
