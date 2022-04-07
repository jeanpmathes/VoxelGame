// <copyright file="Clear.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Gets the world seed.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GetSeed : Command
{
    /// <inheritdoc />
    public override string Name => "get-seed";

    /// <inheritdoc />
    public override string HelpText => "Gets the world seed.";

    /// <exclude />
    public void Invoke()
    {
        int seed = Context.Player.World.Seed;
        Context.Console.WriteResponse($"{seed}");
    }
}
