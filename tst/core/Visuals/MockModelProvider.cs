// <copyright file="MockModelProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Tests.Visuals;

public class MockModelProvider : IModelProvider
{
    public IResourceContext? Context { get; set; }

    public void SetUp() {}

    public Model GetModel(RID identifier, Vector3i? part = null)
    {
        return Model.CreateFallback();
    }
}
