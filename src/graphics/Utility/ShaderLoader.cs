// <copyright file="ShaderLoader.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;

namespace VoxelGame.Graphics.Utility
{
    public class ShaderLoader
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ShaderLoader>();

        private readonly string directory;
        private readonly Dictionary<string, string> includables = new();

        private readonly Regex includePattern = new(@"^#pragma(?: )+include\(""(.+)""\)$");
        private readonly (ISet<Shader> set, string uniform)[] sets;

        public ShaderLoader(string directory, params (ISet<Shader> set, string uniform)[] sets)
        {
            this.directory = directory;
            this.sets = sets;
        }

        public void LoadIncludable(string name, string file)
        {
            includables[name] = File.ReadAllText(Path.Combine(directory, file), Encoding.UTF8);
        }

        public Shader Load(string vert, string frag)
        {
            using var vertReader = new StreamReader(Path.Combine(directory, vert), Encoding.UTF8);
            using var fragReader = new StreamReader(Path.Combine(directory, frag), Encoding.UTF8);

            var shader = new Shader(ProcessSource(vertReader), ProcessSource(fragReader));

            foreach ((ISet<Shader> set, string uniform) in sets)
                if (shader.IsUniformDefined(uniform))
                    set.Add(shader);

            return shader;
        }

        private string ProcessSource(TextReader reader)
        {
            var source = new StringBuilder();

            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                Match match = includePattern.Match(line);

                if (match.Success)
                {
                    string name = match.Groups[groupnum: 1].Value;

                    if (includables.ContainsKey(name)) source.AppendLine(includables[name]);
                    else logger.LogWarning("Cannot resolve shader include for name: {Name}", name);
                }
                else
                {
                    source.AppendLine(line);
                }
            }

            return source.ToString();
        }
    }
}