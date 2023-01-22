// <copyright file="EmitViews.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Threading.Tasks;
using JetBrains.Annotations;
using VoxelGame.Client.Utilities;
using VoxelGame.UI.UserInterfaces;

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
        string path = Context.Player.World.Data.DebugDirectory;

        Task.Run(() =>
        {
            Context.Player.World.EmitViews(path);

            Context.Console.EnqueueResponse($"Emitted views to: {path}",
                new FollowUp("Open folder", () => OS.Start(path)));
        });
    }
}

