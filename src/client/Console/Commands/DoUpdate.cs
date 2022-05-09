// <copyright file="DoUpdate.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Cause a random update to occur for a targeted position.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DoUpdate : Command
{
    /// <inheritdoc />
    public override string Name => "do-update";

    /// <inheritdoc />
    public override string HelpText => "Cause a random update to occur for a targeted position.";

    /// <exclude />
    public void Invoke()
    {
        if (Context.Player.TargetPosition is {} targetPosition) Update(targetPosition);
        else Context.Console.WriteError("No position targeted.");
    }

    /// <exclude />
    public void Invoke(int x, int y, int z)
    {
        Update((x, y, z));
    }

    private void Update(Vector3i position)
    {
        bool success = Context.Player.World.DoRandomUpdate(position);
        if (!success) Context.Console.WriteError("Cannot update at this position.");
    }
}
