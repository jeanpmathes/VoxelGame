// <copyright file="ProgramException.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Utility;

/// <summary>
///     Exception thrown when a a program fails to link.
/// </summary>
public class ProgramException : ApplicationException
{
    /// <summary>
    ///     Creates a new shader exception.
    /// </summary>
    /// <param name="program">The id of the program that caused problems.</param>
    public ProgramException(int program)
    {
        Program = program;
        Info = GL.GetProgramInfoLog(program);
    }

    /// <summary>
    ///     Get the program id.
    /// </summary>
    public int Program { get; }

    /// <summary>
    ///     Get more information about the program problems.
    /// </summary>
    public string Info { get; }

    /// <inheritdoc />
    public override string Message => $"Invalid Program({Program}): {Info}";
}
