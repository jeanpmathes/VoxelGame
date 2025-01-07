// <copyright file="Modifier.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Defines the base class of an image modifier.
///     Modifiers are always applied to single images and can return a sheet of images.
///     One can inherit from this class to create custom modifiers.
///     Custom modifiers are detected by reflection.
/// </summary>
/// <param name="type">The type of this modifier. Used as a key to find the correct modifier.</param>
public abstract class Modifier(String type)
{
    /// <summary>
    ///     The type of this modifier. Used as a key to find the correct modifier.
    /// </summary>
    public String Type { get; } = type;

    /// <summary>
    ///     Modify the given image.
    /// </summary>
    /// <param name="image">The image - can be modified in place.</param>
    /// <param name="parameters">The parameters of the modifier.</param>
    /// <returns>The resulting sheet of images.</returns>
    public Sheet Modify(Image image, IReadOnlyDictionary<String, String> parameters)
    {
        return Modify(image, new Parameters());
    }

    /// <summary>
    ///     Modify the given image.
    /// </summary>
    /// <param name="image">The image - can be modified in place.</param>
    /// <param name="parameters">The parsed parameters of the modifier.</param>
    /// <returns>The resulting sheet of images.</returns>
    protected abstract Sheet Modify(Image image, Parameters parameters);

    /// <summary>
    ///     Wrap an image in a sheet, without copying it.
    /// </summary>
    protected Sheet Wrap(Image image)
    {
        return new Sheet(width: 1, height: 1)
        {
            [x: 0, y: 0] = image
        };
    }

    protected class Parameters {}
}
