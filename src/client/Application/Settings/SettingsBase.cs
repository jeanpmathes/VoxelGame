// <copyright file="SettingsBase.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application.Settings;

/// <summary>
///     Base class helping with the implementation of <see cref="ISettingsProvider" />.
/// </summary>
public abstract partial class SettingsBase
{
    private readonly List<Setting> settings = [];

    /// <summary>
    ///     Adds a setting to the settings list.
    /// </summary>
    /// <param name="name">The name of the setting.</param>
    /// <param name="setting">The setting to add.</param>
    protected void AddSetting(String name, Setting setting)
    {
        settings.Add(setting);

        LogSettingInitialValue(Logger, name, setting.Value);
    }

    /// <summary>
    ///     Get all settings.
    /// </summary>
    public IEnumerable<Setting> Settings => settings;

    #region LOGGING

    /// <summary>
    ///     Provides the logger this class will use.
    /// </summary>
    protected abstract ILogger Logger { get; }

    [LoggerMessage(EventId = Events.ApplicationSettings, Level = LogLevel.Debug, Message = "Setting {Name} is initialized with value: {Value}")]
    private static partial void LogSettingInitialValue(ILogger logger, String name, Object value);

    #endregion LOGGING
}
