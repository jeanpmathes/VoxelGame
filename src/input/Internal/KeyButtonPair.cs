// <copyright file="KeyButtonPair.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame.Input.Internal;

/// <summary>
///     A key-button pair, used for serialization of <see cref="KeyOrButton" />.
/// </summary>
[Serializable]
public class KeyButtonPair
{
    /// <summary>
    ///     Get whether the pair is the default setting, which means that the key and button values can be ignored.
    /// </summary>
    public bool Default { get; set; }

    /// <summary>
    ///     The key value.
    /// </summary>
    public Keys Key { get; set; } = Keys.Unknown;

    /// <summary>
    ///     The button value.
    /// </summary>
    public MouseButton Button { get; set; }

    /// <summary>
    ///     Create a new <see cref="KeyButtonPair" /> with default value.
    /// </summary>
    public static KeyButtonPair DefaultValue => new() { Default = true };
}
