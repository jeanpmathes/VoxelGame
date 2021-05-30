// <copyright file="Shader.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering
{
    public class Shader
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<Shader>();

        private readonly Dictionary<string, int> uniformLocations;

        public int Handle { get; }

        public Shader(string vertName, string fragName)
        {
            string shaderSource = LoadSource(vertName);
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            CompileShader(vertexShader);

            shaderSource = LoadSource(fragName);
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
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);

            if (code != (int)All.True)
            {
                Exception e = new Exception($"Error occurred whilst compiling Shader({shader})");

                Logger.LogCritical(LoggingEvents.ShaderError, e, "Error occurred whilst compiling Shader({shader}): {info}", shader, GL.GetShaderInfoLog(shader));

                throw e;
            }
            else
            {
                Logger.LogDebug("Successfully compiled Shader({shader})", shader);
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);

            if (code != (int)All.True)
            {
                Exception e = new Exception($"Error occurred whilst linking Program({program})");

                Logger.LogCritical(LoggingEvents.ShaderError, e, "Error occurred whilst linking Program({program}): {info}", program, GL.GetProgramInfoLog(program));

                throw e;
            }
            else
            {
                Logger.LogDebug("Successfully linked Program({program})", program);
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        private static string LoadSource(string name)
        {
            using var sr = new StreamReader(Path.Combine("Resources/Shaders", name), Encoding.UTF8);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(uniformLocations[name], data);
        }
    }
}