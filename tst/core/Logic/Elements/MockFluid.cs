// <copyright file="MockFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents.Fluids;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Tests.Logic.Elements;

public class MockFluid() : BasicFluid(
    "Mock Fluid",
    nameof(MockFluid),
    density: 1.0,
    viscosity: 1,
    hasNeutralTint: false,
    TID.MissingTexture);
