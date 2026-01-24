// <copyright file="MockDominantColorProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Tests.Visuals;

public class MockDominantColorProvider : IDominantColorProvider
{
    public IResourceContext? Context { get; set; }

    public void SetUp() {}

    public ColorS GetDominantColor(Int32 index, Boolean isBlock)
    {
        return ColorS.Magenta;
    }
}
