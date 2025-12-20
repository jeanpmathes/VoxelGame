// <copyright file="SettingsBase.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
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
    ///     Get all settings.
    /// </summary>
    public IEnumerable<Setting> Settings => settings;

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

    #region LOGGING

    /// <summary>
    ///     Provides the logger this class will use.
    /// </summary>
    protected abstract ILogger Logger { get; }

    [LoggerMessage(EventId = LogID.Settings + 0, Level = LogLevel.Debug, Message = "Setting {Name} is initialized with value: {Value}")]
    private static partial void LogSettingInitialValue(ILogger logger, String name, Object value);

    #endregion LOGGING
}
