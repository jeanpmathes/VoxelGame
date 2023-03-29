﻿// <copyright file="TextureParameters.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Contains common texture parameters that can be shared between different textures.
/// </summary>
public class TextureParameters
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<TextureParameters>();

    /// <summary>
    ///     The anisotropic filtering level.
    /// </summary>
    public float Anisotropy { get; init; }

    /// <summary>
    ///     Create texture parameters used for textures that are used for rendering objects in the world.
    /// </summary>
    /// <returns>The created texture parameters.</returns>
    internal static TextureParameters CreateForWorld(Application.Client client)
    {
        // todo: remove this class

        return new TextureParameters
        {
            Anisotropy = 0
        };
    }

    /// <summary>
    ///     Set the texture and sampler parameters for the given texture.
    /// </summary>
    /// <param name="texture">The texture to set the parameters for.</param>
    public void SetTextureParameters(int texture)
    {
        //GL.TextureParameter(texture, (TextureParameterName) All.TextureMaxAnisotropy, Anisotropy);
    }
}

