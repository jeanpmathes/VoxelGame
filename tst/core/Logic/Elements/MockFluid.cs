// <copyright file="MockFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents.Fluids;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Tests.Logic.Elements;

public sealed class MockFluid() : BasicFluid("Mock Fluid",
    nameof(MockFluid),
    new Density { KilogramsPerCubicMeter = 1.0 },
    new Viscosity { UpdateTicks = 1 },
    hasNeutralTint: false,
    TID.MissingTexture);
