// <copyright file="LocalPlayerHook.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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

        RemoveSelf();
    }
}
