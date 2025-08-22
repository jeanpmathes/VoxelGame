// <copyright file="EmitWorldInfo.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Emit information about the generated world for debugging.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EmitWorldInfo : Command
{
    /// <inheritdoc />
    public override String Name => "emit-world-info";

    /// <inheritdoc />
    public override String HelpText => "Emit information about the generated world for debugging.";

    /// <exclude />
    public void Invoke()
    {
        DirectoryInfo path = Context.Player.World.Data.DebugDirectory;

        Context.Player.World.EmitWorldInfo(path).OnSuccessfulSync(() =>
        {
            Context.Output.WriteResponse($"Emitted world info to: {path}",
                [new FollowUp("Open folder", () => OS.Start(path))]);
        });
    }
}
