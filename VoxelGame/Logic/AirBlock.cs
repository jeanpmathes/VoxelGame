using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    public class AirBlock : Block
    {
        public AirBlock(string name) : base(name, false)
        {

        }

        public override uint GetMesh(BlockSide side, out float[] vertecies, out uint[] indicies)
        {
            vertecies = null;
            indicies = null;

            return 0;
        }
    }
}
