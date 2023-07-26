// <copyright file="ShaderLoader.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Graphics.Objects;

namespace VoxelGame.Support.Graphics.Utility;

/// <summary>
///     Helps with loading shaders.
/// </summary>
public class ShaderLoader
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ShaderLoader>();

    private readonly DirectoryInfo directory;
    private readonly Dictionary<string, string> includables = new();

    private readonly Regex includePattern = new(@"^#pragma(?: )+include\(""(.+)""\)$");

    private readonly LoadingContext loadingContext;
    private readonly (ISet<Shader> set, string uniform)[] sets;

    /// <summary>
    ///     Create a shader loader.
    /// </summary>
    /// <param name="directory">The directory to load shaders from.</param>
    /// <param name="loadingContext">The loading context.</param>
    /// <param name="sets">Shader sets to fill. Shaders will be added to a set if they contain the specified uniform.</param>
    public ShaderLoader(DirectoryInfo directory, LoadingContext loadingContext, params (ISet<Shader> set, string uniform)[] sets)
    {
        // todo: remove this class, as code processing happens on C++ side now
        // todo: C++ side just has to expose a method to load shaders (given paths), returning handles or error codes
        // todo: loading can than be done by the Shaders.cs class

        this.directory = directory;
        this.loadingContext = loadingContext;
        this.sets = sets;
    }

    /// <summary>
    ///     Load a file that can be included in other shaders.
    /// </summary>
    /// <param name="name">The name of the content. Will be used as marker for including and to construct the file path.</param>
    /// <returns>True if the file was loaded successfully, false otherwise.</returns>
    public bool LoadIncludable(string name)
    {
        FileInfo file = directory.GetFile($"{name}.glsl");

        try
        {
            includables[name] = file.ReadAllText();

            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            loadingContext.ReportFailure(Events.ShaderError, nameof(Shader), file, exception);
            includables[name] = string.Empty;

            return false;
        }
    }

    /// <summary>
    ///     Load a shader.
    /// </summary>
    /// <param name="name">The name of the combined program.</param>
    /// <param name="vert">The name of the vertex shader.</param>
    /// <param name="frag">The name of the fragment shader.</param>
    /// <returns>The loaded shader, or null if an error occurred.</returns>
    public Shader? Load(string name, string vert, string frag)
    {
        string vertex;
        string fragment;

        FileInfo vertFile = directory.GetFile($"{vert}.vert");
        FileInfo fragFile = directory.GetFile($"{frag}.frag");

        try
        {
            using StreamReader vertReader = vertFile.OpenText();
            using StreamReader fragReader = fragFile.OpenText();

            vertex = ProcessSource(vertReader);
            fragment = ProcessSource(fragReader);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            loadingContext.ReportFailure(Events.ShaderError, nameof(Shader), vertFile, exception);
            loadingContext.ReportFailure(Events.ShaderError, nameof(Shader), fragFile, exception);

            return null;
        }

        Shader? shader = Shader.Load(vertex, fragment);

        if (shader == null)
        {
            loadingContext.ReportFailure(Events.ShaderError, nameof(Shader), name, "Failed to compile and link shader");

            return null;
        }

        foreach ((ISet<Shader> set, string uniform) in sets)
            if (shader.IsUniformDefined(uniform))
                set.Add(shader);

        loadingContext.ReportSuccess(Events.ShaderSetup, nameof(Shader), name);

        return shader;
    }

    private string ProcessSource(TextReader reader)
    {
        var source = new StringBuilder();

        while (reader.ReadLine() is {} line)
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
