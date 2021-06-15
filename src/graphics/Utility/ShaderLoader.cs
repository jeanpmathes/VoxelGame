// <copyright file="ShaderLoader.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Graphics.Utility
{
    public class ShaderLoader
    {
        private readonly string directory;

        public ShaderLoader(string directory)
        {
            this.directory = directory;
        }

        public Shader Load(string vert, string frag)
        {
            return new Shader(Path.Combine(directory, vert), Path.Combine(directory, frag));
        }
    }
}