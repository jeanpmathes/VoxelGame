// <copyright file="Setting.cs" company="VoxelGame">
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
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Definition;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Encapsulates a setting, a value that has meaning and can be changed.
/// </summary>
public abstract class Setting
{
    /// <summary>
    ///     Get the name of the setting, in natural (localized) language.
    /// </summary>
    protected abstract String Name { get; }

    /// <summary>
    ///     Provides the untyped value of the setting.
    ///     Use only for debugging purposes.
    /// </summary>
    public abstract Object Value { get; }

    /// <summary>
    ///     The validator for the setting.
    /// </summary>
    public required ISettingsValidator Validator { get; init; } = null!;

    internal void CreateControl(TableRow row, Context context)
    {
        VerticalLayout layout = new(row);

        row.SetCellContents(column: 0, layout);

        Separator separator = new(layout)
        {
            Text = Name
        };

        Control.Used(separator);

        FillControl(layout, context);
    }

    internal virtual void Validate() {}

    private protected abstract void FillControl(ControlBase control, Context context);

    /// <summary>
    ///     Create a setting for key-or-button values.
    /// </summary>
    /// <param name="validator">The settings validator.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="get">Function that gets the current setting value.</param>
    /// <param name="set">Function that sets the current setting value.</param>
    /// <param name="validate">Function that validates the current setting value.</param>
    /// <param name="reset">Function that resets the current setting value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateKeyOrButtonSetting(ISettingsValidator validator, String name,
        Func<VirtualKeys> get, Action<VirtualKeys> set,
        Func<Boolean> validate, Action reset)
    {
        return new KeyOrButtonSetting(name, get, set, validate, reset)
        {
            Validator = validator
        };
    }

    /// <summary>
    ///     Create a setting for integer values.
    /// </summary>
    /// <param name="validator">The settings validator.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateIntegerSetting(ISettingsValidator validator, String name,
        (Func<Int32> get, Action<Int32> set) accessors,
        Int32 min = Int32.MinValue, Int32 max = Int32.MaxValue)
    {
        return new IntegerSetting(name, min, max, accessors.get, accessors.set)
        {
            Validator = validator
        };
    }

    /// <summary>
    ///     Create a setting for color values.
    /// </summary>
    /// <param name="validator">The setting validator.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateColorSetting(ISettingsValidator validator, String name,
        (Func<ColorS> get, Action<ColorS> set) accessors)
    {
        return new ColorSettings(name, accessors.get, accessors.set)
        {
            Validator = validator
        };
    }

    /// <summary>
    ///     Create a setting for values in a float range.
    /// </summary>
    /// <param name="validator">The setting validator.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <param name="min">The minimum value of the setting.</param>
    /// <param name="max">The maximum value of the setting.</param>
    /// <param name="percentage">Whether the value is a percentage and should be displayed as such.</param>
    /// <param name="step">The step size of the slider.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateFloatRangeSetting(ISettingsValidator validator, String name,
        (Func<Single> get, Action<Single> set) accessors,
        Single min = Single.MinValue, Single max = Single.MaxValue,
        Boolean percentage = false, Single? step = null)
    {
        return new FloatRangeSetting(name, min, max, percentage, step, accessors.get, accessors.set)
        {
            Validator = validator
        };
    }

    /// <summary>
    ///     Create a quality setting.
    /// </summary>
    /// <param name="validator">The setting validator.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateQualitySetting(ISettingsValidator validator, String name,
        (Func<Quality> get, Action<Quality> set) accessors)
    {
        return new QualitySetting(name, accessors.get, accessors.set)
        {
            Validator = validator
        };
    }

    /// <summary>
    ///     Create a setting for a boolean value.
    /// </summary>
    /// <param name="validator">The setting validator.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateBooleanSetting(ISettingsValidator validator, String name,
        (Func<Boolean> get, Action<Boolean> set) accessors)
    {
        return new BooleanSetting(name, accessors.get, accessors.set)
        {
            Validator = validator
        };
    }

    /// <summary>
    ///     Create a setting for a size value.
    /// </summary>
    /// <param name="validator">The setting validator.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <param name="current">
    ///     Function to get the current value used by the application, if it is possible to change it independently of the
    ///     setting.
    /// </param>
    /// <returns>The created setting.</returns>
    public static Setting CreateSizeSetting(ISettingsValidator validator, String name,
        (Func<Vector2i> get, Action<Vector2i> set) accessors, Func<Vector2i>? current = null)
    {
        return new SizeSetting(name, accessors.get, accessors.set, current)
        {
            Validator = validator
        };
    }
}
