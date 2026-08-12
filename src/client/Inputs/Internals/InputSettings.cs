// <copyright file="InputSettings.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Configuration;
using Microsoft.Extensions.Logging;
using Properties;
using VoxelGame.Graphics.Input;
using VoxelGame.Logging;
using VoxelGame.Presentation.Legacy.Providers;
using VoxelGame.Presentation.Legacy.Settings;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs.Internals;

/// <summary>
///     Provides configurable <see cref="Setting" />s for all keybinds that are persisted in the application settings and
///     can be presented to the user.
/// </summary>
internal sealed partial class InputSettings
{
    private readonly BindingLayer applicationLayer;
    private readonly BindingLayer gameLayer;

    private readonly Bindings bindings;
    private readonly ISettingsValidator validator;

    private readonly List<Setting> settings = [];
    private readonly Dictionary<KeyOrButtonCombination, Int32> usageCounts = new();

    /// <summary>
    ///     Apply saved input combinations to the runtime bindings and create the settings used to change them.
    /// </summary>
    /// <param name="bindings">The bindings whose combinations are exposed as settings.</param>
    /// <param name="applicationLayer">The application layer used to capture new combinations and apply its changes.</param>
    /// <param name="gameLayer">The game layer used to apply its changes.</param>
    /// <param name="validator">The validator used to revalidate all keybind settings after one changes.</param>
    internal InputSettings(Bindings bindings, BindingLayer applicationLayer, BindingLayer gameLayer, ISettingsValidator validator)
    {
        this.applicationLayer = applicationLayer;
        this.gameLayer = gameLayer;

        this.validator = validator;
        this.bindings = bindings;

        foreach (Binding binding in bindings.All)
            AddUsage(binding.Combination);

        InitializePersistence();
        InitializePresentation();
    }

    /// <summary>
    ///     Get the keybind settings in keybind definition order.
    /// </summary>
    internal IEnumerable<Setting> All => settings;

    private void InitializePersistence()
    {
        foreach (Binding binding in bindings.All)
        {
            String key = PropertyName(binding.Definition);

            SettingsProperty property = new(key)
            {
                PropertyType = typeof(OptionalKeyOrButtonCombination),
                IsReadOnly = false,
                DefaultValue = "",
                Provider = Settings.Default.Providers["LocalFileSettingsProvider"],
                SerializeAs = SettingsSerializeAs.Xml
            };

            property.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());
            Settings.Default.Properties.Add(property);
        }

        Settings.Default.Reload();

        foreach (Binding binding in bindings.All)
        {
            OptionalKeyOrButtonCombination? stored = (OptionalKeyOrButtonCombination) Settings.Default[PropertyName(binding.Definition)];

            if (stored is not {UseDefault: false}) continue;

            KeyOrButtonCombination combination = new(stored.KeyOrButton, stored.Modifiers);

            if (binding.Definition.Layer.Allows(combination))
                Rebind(binding, combination, useDefault: false);
            else
                Settings.Default[PropertyName(binding.Definition)] = binding.Definition.Default.ToSettingsValue(useDefault: true);
        }

        Settings.Default.Save();
        LogFinishedInitializingKeybindSettings(logger);
    }

    private void InitializePresentation()
    {
        foreach (Binding binding in bindings.All)
        {
            Setting setting = Setting.CreateKeyOrButtonSetting(
                validator,
                binding.Definition.Name,
                () => binding.Combination,
                combination => Rebind(binding, combination, useDefault: false),
                callback => applicationLayer.BeginCapture(binding.Definition.Layer, callback),
                () => GetUsageCount(binding.Combination) <= 1,
                () => Rebind(binding, binding.Definition.Default, useDefault: true));

            settings.Add(setting);
        }
    }

    private void Rebind(Binding binding, KeyOrButtonCombination combination, Boolean useDefault)
    {
        if (!binding.Definition.Layer.Allows(combination)) return;

        RemoveUsage(binding.Combination);
        GetLayer(binding.Definition.Layer).Rebind(binding, combination);
        AddUsage(combination);

        Settings.Default[PropertyName(binding.Definition)] = combination.ToSettingsValue(useDefault);
        Settings.Default.Save();

        LogRebindKeybind(logger, binding.Definition, combination);
    }

    private BindingLayer GetLayer(Layer layer)
    {
        return layer switch
        {
            Layer.Application => applicationLayer,
            Layer.Game => gameLayer,
            _ => throw Exceptions.UnsupportedEnumValue(layer)
        };
    }

    private void AddUsage(KeyOrButtonCombination combination)
    {
        usageCounts.TryGetValue(combination, out Int32 count);
        usageCounts[combination] = count + 1;

        if (count > 0) LogCombinationUsedByMultipleBindings(logger, combination);
    }

    private void RemoveUsage(KeyOrButtonCombination combination)
    {
        Int32 count = usageCounts[combination] - 1;

        if (count == 0)
            usageCounts.Remove(combination);
        else
            usageCounts[combination] = count;
    }

    private Int32 GetUsageCount(KeyOrButtonCombination combination)
    {
        return usageCounts.GetValueOrDefault(combination);
    }

    private static String PropertyName(Keybind definition)
    {
        return $"input_{definition}";
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<InputSettings>();

    [LoggerMessage(EventId = LogID.InputSettings + 0, Level = LogLevel.Information, Message = "Finished initializing keybind settings")]
    private static partial void LogFinishedInitializingKeybindSettings(ILogger logger);

    [LoggerMessage(EventId = LogID.InputSettings + 1, Level = LogLevel.Warning, Message = "Combination '{Combination}' is used by multiple bindings")]
    private static partial void LogCombinationUsedByMultipleBindings(ILogger logger, KeyOrButtonCombination combination);

    [LoggerMessage(EventId = LogID.InputSettings + 2, Level = LogLevel.Information, Message = "Rebind '{Bind}' to: {Combination}")]
    private static partial void LogRebindKeybind(ILogger logger, Keybind bind, KeyOrButtonCombination combination);

    #endregion LOGGING
}
