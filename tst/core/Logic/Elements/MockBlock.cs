// <copyright file="MockBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Tests.Logic.Elements;

public class MockBlock() : Block(id: 0, namedID: nameof(MockBlock), name: "Mock Block")
{
    protected override void OnValidate()
    {
        
    }

    public override Meshable Meshable => Meshable.Unmeshed;
    
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals)
    {
        
    }
    
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        
    }
}
