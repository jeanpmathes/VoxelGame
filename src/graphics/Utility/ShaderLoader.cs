// <copyright file="ShaderLoader.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.IO;
using System.Text;
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
            using var vertReader = new StreamReader(Path.Combine(directory, vert), Encoding.UTF8);
            using var fragReader = new StreamReader(Path.Combine(directory, frag), Encoding.UTF8);

            var shader = new Shader(vertReader.ReadToEnd(), fragReader.ReadToEnd());

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