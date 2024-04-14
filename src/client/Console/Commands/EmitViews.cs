// <copyright file="EmitViews.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities;
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
    public override String Name => "emit-views";

    /// <inheritdoc />
    public override String HelpText => "Emit views of the generated map for debugging.";

    /// <exclude />
    public void Invoke()
    {
        DirectoryInfo path = Context.Player.World.Data.DebugDirectory;

        Task.Run(() =>
        {
            Context.Player.World.EmitViews(path);

            Context.Console.EnqueueResponse($"Emitted views to: {path}",
                new FollowUp("Open folder", () => OS.Start(path)));
        });
    }
}
