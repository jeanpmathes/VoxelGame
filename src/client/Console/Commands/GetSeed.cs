﻿// <copyright file="Clear.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    public override String Name => "get-seed";

    /// <inheritdoc />
    public override String HelpText => "Gets the world seed.";

    /// <exclude />
    public void Invoke()
    {
        (Int32 upper, Int32 lower) = Context.Player.World.Seed;
        Context.Console.WriteResponse($"({upper}, {lower})");
    }
}
