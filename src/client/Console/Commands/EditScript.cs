// <copyright file="EditScript.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using VoxelGame.Client.Utilities;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Open a script for editing.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EditScript : Command
{
    /// <inheritdoc />
    public override string Name => "edit-script";

    /// <inheritdoc />
    public override string HelpText => "Edit a ready script by opening it.";

    /// <exclude />
    public void Invoke(string name)
    {
        Do(Context, name);
    }

    /// <summary>
    ///     Perform a run of the command, allowing to edit a script.
    /// </summary>
    /// <param name="context">The context to use.</param>
    /// <param name="name">The name of the script to edit.</param>
    public static void Do(Context context, string name)
    {
        string? path = context.Player.World.Data.CreateScript(name, "");

        if (path == null) return;

        OS.Start(path);
    }
}
