// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Graphics;

/// <summary>
///     Utility class that contains utility methods to get values from the current rendering context.
/// </summary>
public static class Context
{
    /// <summary>
    ///     Get the max available texture samples.
    /// </summary>
    public static int MaxTextureSamples => GL.GetInteger(GetPName.MaxSamples);

    /// <summary>
    ///     Get the max anisotropic filtering level.
    /// </summary>
    public static float MaxAnisotropy => GL.GetFloat((GetPName) All.MaxTextureMaxAnisotropy);
}

