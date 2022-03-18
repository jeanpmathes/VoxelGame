// <copyright file="ShaderException.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Utility;

/// <summary>
///     Exception thrown when a shader fails to compile.
/// </summary>
public class ShaderException : Exception
{
    /// <summary>
    ///     Creates a new shader exception.
    /// </summary>
    /// <param name="shader">The id of the shader that caused problems.</param>
    public ShaderException(int shader)
    {
        Shader = shader;
        Info = GL.GetShaderInfoLog(shader);
    }

    /// <summary>
    ///     Get the shader id.
    /// </summary>
    public int Shader { get; }

    /// <summary>
    ///     Get more information about the shader problems.
    /// </summary>
    public string Info { get; }

    /// <inheritdoc />
    public override string Message => $"Invalid Shader({Shader}): {Info}";
}
