// <copyright file="MockTextureBundle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Tests.Visuals;

public class MockTextureBundle : ITextureIndexProvider, IDominantColorProvider
{
    public Color GetDominantColor(Int32 index)
    {
        return Color.Black;
    }

    public Int32 GetTextureIndex(String name)
    {
        return 0;
    }
}
