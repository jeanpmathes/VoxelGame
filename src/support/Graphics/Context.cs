// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Graphics;

/// <summary>
///     Utility class that contains utility methods to get values from the current rendering context.
/// </summary>
public static class Context // todo: maybe remove
{
    /// <summary>
    ///     Get the max available texture samples.
    /// </summary>
    public static int MaxTextureSamples => 1; // todo: implement, old: GL.GetInteger(GetPName.MaxSamples);

    /// <summary>
    ///     Get the max anisotropic filtering level.
    /// </summary>
    public static float MaxAnisotropy => 0.0f; // todo: implement, old: GL.GetFloat((GetPName) All.MaxTextureMaxAnisotropy);
}
