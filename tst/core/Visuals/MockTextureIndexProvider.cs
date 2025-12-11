// <copyright file="MockTextureIndexProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Tests.Visuals;

public class MockTextureIndexProvider : ITextureIndexProvider
{
    public IResourceContext? Context { get; set; }

    public void SetUp() {}

    public Int32 GetTextureIndex(TID identifier)
    {
        return ITextureIndexProvider.MissingTextureIndex;
    }
}
