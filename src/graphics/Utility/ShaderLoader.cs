// <copyright file="ShaderLoader.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.IO;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Graphics.Utility
{
    public class ShaderLoader
    {
        private readonly string directory;
        private readonly (ISet<Shader> set, string uniform)[] sets;

        public ShaderLoader(string directory, params (ISet<Shader> set, string uniform)[] sets)
        {
            this.directory = directory;
            this.sets = sets;
        }

        public Shader Load(string vert, string frag)
        {
            var shader = new Shader(Path.Combine(directory, vert), Path.Combine(directory, frag));

            foreach ((ISet<Shader> set, string uniform) in sets)
            {
                if (shader.IsUniformDefined(uniform))
                {
                    set.Add(shader);
                }
            }

            return shader;
        }
    }
}