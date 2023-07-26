// <copyright file="SettingChangedArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Client.Application;

/// <summary>
///     Arguments passed to a setting changed event.
/// </summary>
/// <param name="Settings">The settings object.</param>
/// <param name="OldValue">The old value of the setting.</param>
/// <param name="NewValue">The new value of the setting.</param>
/// <typeparam name="T">The type of the setting value.</typeparam>
public record SettingChangedArgs<T>(GeneralSettings Settings, T OldValue, T NewValue);
