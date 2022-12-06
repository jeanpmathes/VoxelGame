// <copyright file="GetDistance.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using OpenTK.Mathematics;

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
    public void Invoke(float x, float y, float z)
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
        double distance = (position - Context.Player.Position).Length;

        switch (distance)
        {
            case < 1:
                Context.Console.WriteResponse($"Distance: {distance * 1000:F2} mm");

                break;
            case < 100000:
                Context.Console.WriteResponse($"Distance: {distance:F2} m");

                break;
            default:
                Context.Console.WriteResponse($"Distance: {distance / 1000:F2} km");

                break;
        }
    }
}
