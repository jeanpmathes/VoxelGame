﻿// <copyright file="GetDistance.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Get the distance to a specified point or target.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GetDistance : Command
{
    /// <inheritdoc />
    public override string Name => "get-distance";

    /// <inheritdoc />
    public override string HelpText => "Get the distance to a specified point or target.";

    /// <exclude />
    public void Invoke(double x, double y, double z)
    {
        DetermineDistance((x, y, z));
    }

    /// <exclude />
    public void Invoke(string target)
    {
        if (GetNamedPosition(target) is {} position) DetermineDistance(position);
        else Context.Console.WriteError($"Unknown target: {target}");
    }

    private void DetermineDistance(Vector3d position)
    {
        Length distance = new()
        {
            Meters = (position - Context.Player.Position).Length
        };

        Context.Console.WriteResponse($"Distance: {distance}");
    }
}
