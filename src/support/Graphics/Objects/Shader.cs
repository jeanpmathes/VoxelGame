// <copyright file="Shader.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Logging;

namespace VoxelGame.Support.Graphics.Objects;

/// <summary>
///     A shader.
/// </summary>
public class Shader
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Shader>();

    private readonly Dictionary<string, int> uniformLocations = new();

    private Shader(int handle)
    {
        // todo: port to DirectX (think how to unify RT shader and Raster shaders, as they are loaded differently)
        // todo: maybe don't unify them but have separate shader classes for raster and rt shaders
        // todo: a useful abstraction would potentialy be:
        //   - for raster shaders: a raster pipeline object that is created using shader paths and some config data
        //   - for rt shaders: material objects that are added to the central rt pipeline, all shaders that are part of it are registered in advance and materials just contain the symbol names

        // todo: idea for uniforms: shaders are generic for a struct that is then passed to the shader as constant buffer

        Handle = handle;

        // GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);
        //
        // for (var i = 0; i < numberOfUniforms; i++)
        // {
        //     string key = GL.GetActiveUniform(handle, i, out _, out _);
        //     int location = GL.GetUniformLocation(handle, key);
        //
        //     uniformLocations.Add(key, location);
        // }
        //
        // GL.UseProgram(program: 0);
    }

    private int Handle { get; }

    /// <summary>
    ///     Create a new shader from given source files.
    /// </summary>
    /// <param name="vertSource">The path to the vertex shader source file.</param>
    /// <param name="fragSource">The path to the fragment shader source file.</param>
    /// <returns>The created shader, or null if an error occurred.</returns>
    public static Shader? Load(string vertSource, string fragSource)
    {
        // GL.EnableVertexAttribArray(index: 0);
        //
        // int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        // GL.ShaderSource(vertexShader, vertSource);
        // bool isVertexValid = CompileShader(vertexShader);
        //
        // int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        // GL.ShaderSource(fragmentShader, fragSource);
        // bool isFragmentValid = CompileShader(fragmentShader);

        // todo: ensure that shader loading in C++ still yields nice compile errors into the log

        Shader? shader = null;

        /*if (isVertexValid && isFragmentValid)
        {
            int handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);

            bool isProgramValid = LinkProgram(handle);

            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);

            if (isProgramValid) shader = new Shader(handle);
        }

        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);

        GL.UseProgram(program: 0);*/

        return shader;
    }

    private static bool CompileShader(int shader)
    {
        /*GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);

        if (code != (int) All.True)
        {
            logger.LogCritical(
                Events.ShaderError,
                "Error occurred whilst compiling Shader({Shader}): {Info}",
                shader,
                GL.GetShaderInfoLog(shader));

            return false;
        }*/

        logger.LogDebug(Events.ShaderSetup, "Successfully compiled Shader({Shader})", shader);

        return true;
    }

    private static bool LinkProgram(int program)
    {
        /*GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);

        if (code != (int) All.True)
        {
            logger.LogCritical(
                Events.ShaderError,
                "Error occurred whilst linking Program({Program}): {Info}",
                program,
                GL.GetProgramInfoLog(program));

            return false;
        }*/

        logger.LogDebug(Events.ShaderSetup, "Successfully linked Program({Program})", program);

        return true;
    }

    /// <summary>
    ///     Prepare the shader for usage.
    /// </summary>
    public void Use()
    {
        //GL.UseProgram(Handle);
    }

    /// <summary>
    ///     Get an attribute location.
    /// </summary>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute location.</returns>
    public int GetAttributeLocation(string attributeName)
    {
        return 0; //return GL.GetAttribLocation(Handle, attributeName);
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
        // GL.UseProgram(Handle);
        // GL.Uniform1(uniformLocations[name], data);
    }

    /// <summary>
    ///     Set a uniform float on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    public void SetFloat(string name, float data)
    {
        // GL.UseProgram(Handle);
        // GL.Uniform1(uniformLocations[name], data);
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
        // GL.UseProgram(Handle);
        // GL.UniformMatrix4(uniformLocations[name], transpose: true, ref data);
    }

    /// <summary>
    ///     Set a uniform Vector3d on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    public void SetVector3(string name, Vector3 data)
    {
        // GL.UseProgram(Handle);
        // GL.Uniform3(uniformLocations[name], data);
    }

    /// <summary>
    ///     Set a uniform Color4 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The data to set.</param>
    public void SetColor4(string name, Color4 data)
    {
        // GL.UseProgram(Handle);
        // GL.Uniform4(uniformLocations[name], data);
    }

    /// <summary>
    ///     Delete the shader.
    /// </summary>
    public void Delete()
    {
        // GL.DeleteProgram(Handle);
    }
}

