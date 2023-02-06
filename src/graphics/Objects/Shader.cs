// <copyright file="Shader.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Utility;
using VoxelGame.Logging;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A shader.
/// </summary>
public class Shader
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Shader>();

    private readonly Dictionary<string, int> uniformLocations;

    /// <summary>
    ///     Create a new shader from given source files.
    /// </summary>
    /// <param name="vertSource">The path to the vertex shader source file.</param>
    /// <param name="fragSource">The path to the fragment shader source file.</param>
    public Shader(string vertSource, string fragSource)
    {
        GL.EnableVertexAttribArray(index: 0);

        string shaderSource = vertSource;
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, shaderSource);
        CompileShader(vertexShader);

        shaderSource = fragSource;
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, shaderSource);
        CompileShader(fragmentShader);

        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);

        LinkProgram(Handle);

        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);

        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);
        uniformLocations = new Dictionary<string, int>();

        for (var i = 0; i < numberOfUniforms; i++)
        {
            string key = GL.GetActiveUniform(Handle, i, out _, out _);
            int location = GL.GetUniformLocation(Handle, key);

            uniformLocations.Add(key, location);
        }

        GL.UseProgram(program: 0);
    }

    private int Handle { get; }

    private static void CompileShader(int shader)
    {
        GL.CompileShader(shader);

        // Check for compilation errors
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);

        if (code != (int) All.True)
        {
            var e = new ShaderException(shader);

            logger.LogCritical(
                Events.ShaderError,
                e,
                "Error occurred whilst compiling Shader({Shader}): {Info}",
                e.Shader,
                e.Info);

            throw e;
        }

        logger.LogDebug(Events.ShaderSetup, "Successfully compiled Shader({Shader})", shader);
    }

    private static void LinkProgram(int program)
    {
        GL.LinkProgram(program);

        // Check for linking errors
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);

        if (code != (int) All.True)
        {
            var e = new ProgramException(program);

            logger.LogCritical(
                Events.ShaderError,
                e,
                "Error occurred whilst linking Program({Program}): {Info}",
                e.Program,
                e.Info);

            throw e;
        }

        logger.LogDebug(Events.ShaderSetup, "Successfully linked Program({Program})", program);
    }

    /// <summary>
    ///     Prepare the shader for usage.
    /// </summary>
    public void Use()
    {
        GL.UseProgram(Handle);
    }

    /// <summary>
    ///     Get an attribute location.
    /// </summary>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute location.</returns>
    public int GetAttributeLocation(string attributeName)
    {
        return GL.GetAttribLocation(Handle, attributeName);
    }

    /// <summary>
    ///     Check if a uniform is present.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <returns>True if it is defined.</returns>
    public bool IsUniformDefined(string name)
    {
        return uniformLocations.ContainsKey(name);
    }

    /// <summary>
    ///     Set a uniform int on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    public void SetInt(string name, int data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(uniformLocations[name], data);
    }

    /// <summary>
    ///     Set a uniform float on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    public void SetFloat(string name, float data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(uniformLocations[name], data);
    }

    /// <summary>
    ///     Set a uniform Matrix4 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    /// <remarks>
    ///     <para>
    ///         The matrix is transposed before being sent to the shader.
    ///     </para>
    /// </remarks>
    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(uniformLocations[name], transpose: true, ref data);
    }

    /// <summary>
    ///     Set a uniform Vector3d on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    public void SetVector3(string name, Vector3 data)
    {
        GL.UseProgram(Handle);
        GL.Uniform3(uniformLocations[name], data);
    }

    /// <summary>
    ///     Set a uniform Color4 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    public void SetColor4(string name, Color4 data)
    {
        GL.UseProgram(Handle);
        GL.Uniform4(uniformLocations[name], data);
    }

    /// <summary>
    ///     Delete the shader.
    /// </summary>
    public void Delete()
    {
        GL.DeleteProgram(Handle);
    }
}


