// <copyright file="OptionalKey.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Input;

/// <summary>
///     Stores are key or the default state, used for serialization.
/// </summary>
[Serializable]
public class OptionalKey
{
    /// <summary>
    ///     Get or set whether the key should use the default value.
    /// </summary>
    public bool Default { get; set; } = true;

    /// <summary>
    ///     The key, if <see cref="Default" /> is false, or an invalid value if <see cref="Default" /> is true.
    /// </summary>
    public VirtualKeys Key { get; set; } = VirtualKeys.Undefined;

    /// <summary>
    ///     Create a new <see cref="OptionalKey" /> with default value.
    /// </summary>
    public static OptionalKey DefaultValue => new() {Default = true};
}

/// <summary>
///     Extension methods for <see cref="OptionalKey" />.
/// </summary>
public static class OptionalKeyExtensions
{
    /// <summary>
    ///     Get an optional key value to serialize a <see cref="VirtualKeys" /> value.
    /// </summary>
    /// <param name="key">The key to serialize.</param>
    /// <param name="isDefault">Whether to use a default value instead.</param>
    /// <returns>The optional key.</returns>
    public static OptionalKey GetSettings(this VirtualKeys key, bool isDefault)
    {
        return isDefault ? OptionalKey.DefaultValue : new OptionalKey {Default = false, Key = key};
    }
}

