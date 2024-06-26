﻿// <copyright file="SetPhysics.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Allows to enable or disable physics for the player.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetPhysics : Command
{
    /// <inheritdoc />
    public override String Name => "set-physics";

    /// <inheritdoc />
    public override String HelpText => "Set whether to enable physics for the player.";

    /// <exclude />
    public void Invoke(Boolean enabled)
    {
        Do(Context, enabled);
    }

    /// <summary>
    ///     Externally simulate a command invocation, setting the physics state.
    /// </summary>
    public static void Do(Context context, Boolean enabled)
    {
        context.Player.DoPhysics = enabled;
    }
}
