// <copyright file="GetDistance.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    public override String Name => "get-distance";

    /// <inheritdoc />
    public override String HelpText => "Get the distance to a specified point or target.";

    /// <exclude />
    public void Invoke(Double x, Double y, Double z)
    {
        DetermineDistance((x, y, z));
    }

    /// <exclude />
    public void Invoke(String target)
    {
        if (GetNamedPosition(target) is {} position) DetermineDistance(position);
        else Context.Output.WriteError($"Unknown target: {target}");
    }

    private void DetermineDistance(Vector3d position)
    {
        Length distance = new()
        {
            Meters = (position - Context.Player.Body.Transform.Position).Length
        };

        Context.Output.WriteResponse($"Distance: {distance}");
    }
}
