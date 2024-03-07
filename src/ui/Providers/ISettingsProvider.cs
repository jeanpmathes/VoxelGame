// <copyright file="ISettingsProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.UI.Settings;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provide the user interface with settings for a given category.
/// </summary>
public interface ISettingsProvider : ISettingsValidator
{
    /// <summary>
    ///     The name of the settings category.
    /// </summary>
    public static abstract string Category { get; }

    /// <summary>
    ///     A description for the settings category.
    /// </summary>
    public static abstract string Description { get; }

    /// <summary>
    ///     Get all settings for this category.
    /// </summary>
    public IEnumerable<Setting> Settings { get; }

    /// <inheritdoc />
    void ISettingsValidator.Validate()
    {
        Validate();
    }

    /// <summary>
    ///     Validate the current settings.
    /// </summary>
    public new void Validate()
    {
        foreach (Setting setting in Settings) setting.Validate();
    }
}

/// <summary>
///     Offers a way to validate all settings of a category.
/// </summary>
public interface ISettingsValidator
{
    /// <summary>
    ///     Validate the current settings.
    /// </summary>
    public void Validate();
}

/// <summary>
///     A settings provider object, based on the <see cref="ISettingsProvider" /> interface.
/// </summary>
public class SettingsProvider
{
    /// <summary>
    ///     Create a new settings provider.
    /// </summary>
    protected SettingsProvider() {}

    /// <summary>
    ///     See <see cref="ISettingsProvider.Category" />.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    ///     See <see cref="ISettingsProvider.Description" />.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     See <see cref="ISettingsProvider.Settings" />.
    /// </summary>
    public required IEnumerable<Setting> Settings { get; init; }

    /// <summary>
    ///     Wrap a settings provider object into a form usable by the user interface.
    /// </summary>
    public static SettingsProvider Wrap<T>(T provider) where T : ISettingsProvider
    {
        return new SettingsProviderWrapper<T>(provider);
    }

    /// <summary>
    ///     Validate the current settings.
    /// </summary>
    public void Validate()
    {
        foreach (Setting setting in Settings) setting.Validate();
    }
}

/// <summary>
///     Wraps a settings provider object into a form usable by the user interface.
/// </summary>
/// <typeparam name="T">The type of the settings provider.</typeparam>
internal class SettingsProviderWrapper<T> : SettingsProvider where T : ISettingsProvider
{
    /// <summary>
    ///     Create a new settings provider wrapper.
    /// </summary>
    /// <param name="provider">The settings provider to wrap.</param>
    [SetsRequiredMembers]
    internal SettingsProviderWrapper(T provider)
    {
        Category = T.Category;
        Description = T.Description;

        Settings = provider.Settings;
    }
}
