// <copyright file="EditReadyScript.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Edit the ready script that is executed on world start.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EditReadyScript : Command
{
    /// <inheritdoc />
    public override string Name => "edit-readyscript";

    /// <inheritdoc />
    public override string HelpText => "Edit the ready script that is executed on world start.";

    /// <exclude />
    public void Invoke()
    {
        EditScript.Do(Context, GameConsole.WorldReadyScript);
    }
}
