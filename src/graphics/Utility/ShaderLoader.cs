// <copyright file="ShaderLoader.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;

namespace VoxelGame.Graphics.Utility;

/// <summary>
///     Helps with loading shaders.
/// </summary>
public class ShaderLoader
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ShaderLoader>();

    private readonly DirectoryInfo directory;
    private readonly Dictionary<string, string> includables = new();

    private readonly Regex includePattern = new(@"^#pragma(?: )+include\(""(.+)""\)$");
    private readonly (ISet<Shader> set, string uniform)[] sets;

    /// <summary>
    ///     Create a shader loader.
    /// </summary>
    /// <param name="directory">The directory to load shaders from.</param>
    /// <param name="sets">Shader sets to fill. Shaders will be added to a set if they contain the specified uniform.</param>
    public ShaderLoader(DirectoryInfo directory, params (ISet<Shader> set, string uniform)[] sets)
    {
        this.directory = directory;
        this.sets = sets;
    }

    /// <summary>
    ///     Load a file that can be included in other shaders.
    /// </summary>
    /// <param name="name">The name of the content. Will be used as marker for including.</param>
    /// <param name="file">The path to the file.</param>
    public void LoadIncludable(string name, string file)
    {
        includables[name] = FileSystem.GetFilePath(directory, file).ReadAllText();
    }

    /// <summary>
    ///     Load a shader.
    /// </summary>
    /// <param name="vert">The name of the vertex shader.</param>
    /// <param name="frag">The name of the fragment shader.</param>
    /// <returns>The loaded shader.</returns>
    public Shader Load(string vert, string frag)
    {
        using StreamReader vertReader = FileSystem.GetFilePath(directory, vert).OpenText();
        using StreamReader fragReader = FileSystem.GetFilePath(directory, frag).OpenText();

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
                else logger.LogWarning(Events.ShaderError, "Cannot resolve shader include for name: {Name}", name);
            }
            else
            {
                source.AppendLine(line);
            }
        }

        return source.ToString();
    }
}
