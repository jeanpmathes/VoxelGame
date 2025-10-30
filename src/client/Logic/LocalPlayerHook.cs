// <copyright file="LocalPlayerHook.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Actors;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Hooks the local player to the world logic.
/// </summary>
public partial class LocalPlayerHook : WorldComponent
{
    [Constructible]
    private LocalPlayerHook(Core.Logic.World subject, Player player) : base(subject)
    {
        Player = player;
        Player.OnAdd(subject);
    }

    /// <summary>
    ///     Get the local player of the world.
    /// </summary>
    public Player Player { get; }
    
    /// <inheritdoc />
    public override void OnActivate(Object? sender, EventArgs e)
    {
        Player.Activate();
    }

    /// <inheritdoc />
    public override void OnDeactivate(Object? sender, EventArgs e)
    {
        Player.Deactivate();
    }

    /// <inheritdoc />
    public override void OnTerminate(Object? sender, EventArgs e)
    {
        Player.OnRemove();
    }
}
