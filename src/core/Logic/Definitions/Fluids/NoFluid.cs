// <copyright file="NoFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    /// <param name="namedID">The named ID.</param>
    public NoFluid(String name, String namedID) :
        base(
            name,
            namedID,
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
    protected override void ScheduledUpdate(World world, Vector3i position, FluidInstance instance) {}
}
