// <copyright file="EditReadyScript.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Edit the ready script that is executed on world start.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EditReadyScript : Command
{
    /// <inheritdoc />
    public override String Name => "edit-readyscript";

    /// <inheritdoc />
    public override String HelpText => "Edit the ready script that is executed on world start.";

    /// <exclude />
    public void Invoke()
    {
        EditScript.Do(Context, SessionConsole.WorldReadyScript);
    }
}
