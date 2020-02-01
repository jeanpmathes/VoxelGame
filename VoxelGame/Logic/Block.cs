using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    /// <summary>
    /// The basic block class. Blocks are used to construct the world.
    /// </summary>
    public abstract class Block
    {
        public string Name { get; private set; }
        public bool IsSolid { get; private set; }

        protected int vertexBufferObject;
        protected int elementBufferObject;
        protected int vertexArrayObject;

        protected Shader shader;

        public Block(string name, bool isSolid)
        {
            Name = name;
            IsSolid = isSolid;
        }

        public abstract uint GetMesh(BlockSide side, out float[] vertecies, out uint[] indicies);
    }
}
