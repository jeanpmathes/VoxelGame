// <copyright file="LocalPlayerHook.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Client.Actors;
using VoxelGame.Core.Logic;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Logic;

/// <summary>
/// Hooks the local player to the world logic.
/// </summary>
public class LocalPlayerHook : WorldComponent, IConstructible<Core.Logic.World, Player, LocalPlayerHook>
{
    private LocalPlayerHook(Core.Logic.World subject, Player player) : base(subject) 
    {
        Player = player;
        Player.OnAdd(subject);
    }

    /// <inheritdoc />
    public static LocalPlayerHook Construct(Core.Logic.World input1, Player input2)
    {
        return new LocalPlayerHook(input1, input2);
    }
    
    /// <summary>
    /// Get the local player of the world.
    /// </summary>
    public Player Player { get; }

    /// <inheritdoc />
    public override void OnActivate()
    {
        Player.Activate();
    }

    /// <inheritdoc />
    public override void OnDeactivate()
    {
        Player.Deactivate();
    }

    /// <inheritdoc />
    public override void OnTerminate()
    {
        Player.OnRemove();
    }
}
