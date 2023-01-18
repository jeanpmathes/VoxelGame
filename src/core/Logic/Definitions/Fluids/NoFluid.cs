// <copyright file="NoFluid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Fluids;

/// <summary>
///     This fluid represents the absence of fluids.
/// </summary>
public class NoFluid : Fluid
{
    /// <summary>
    ///     Creates a new <see cref="NoFluid" />.
    /// </summary>
    /// <param name="name">The name of the fluid.</param>
    /// <param name="namedId">The named ID.</param>
    public NoFluid(string name, string namedId) :
        base(
            name,
            namedId,
            AirDensity,
            viscosity: 1,
            checkContact: false,
            receiveContact: false,
            RenderType.NotRendered) {}

    /// <inheritdoc />
    protected override FluidMeshData GetMeshData(FluidMeshInfo info)
    {
        return FluidMeshData.Empty;
    }

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, FluidLevel level, bool isStatic) {}
}

