// <copyright file="Setting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Support.Definition;
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
    ///     Get the name of the setting.
    /// </summary>
    protected abstract string Name { get; }

    /// <summary>
    ///     The provider which provided this setting.
    /// </summary>
    protected ISettingsProvider Provider { get; private set; } = null!;

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
    /// <param name="provider">The settings provider.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="get">Function that gets the current setting value.</param>
    /// <param name="set">Function that sets the current setting value.</param>
    /// <param name="validate">Function that validates the current setting value.</param>
    /// <param name="reset">Function that resets the current setting value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateKeyOrButtonSetting(ISettingsProvider provider, string name,
        Func<VirtualKeys> get, Action<VirtualKeys> set,
        Func<bool> validate, Action reset)
    {
        return new KeyOrButtonSetting(name, get, set, validate, reset)
        {
            Provider = provider
        };
    }

    /// <summary>
    ///     Create a setting for integer values.
    /// </summary>
    /// <param name="provider">The settings provider.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateIntegerSetting(ISettingsProvider provider, string name,
        (Func<int> get, Action<int> set) accessors,
        int min = int.MinValue, int max = int.MaxValue)
    {
        return new IntegerSetting(name, min, max, accessors.get, accessors.set)
        {
            Provider = provider
        };
    }

    /// <summary>
    ///     Create a setting for color values.
    /// </summary>
    /// <param name="provider">The setting provider.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateColorSetting(ISettingsProvider provider, string name,
        (Func<Color> get, Action<Color> set) accessors)
    {
        return new ColorSettings(name, accessors.get, accessors.set)
        {
            Provider = provider
        };
    }

    /// <summary>
    ///     Create a setting for values in a float range.
    /// </summary>
    /// <param name="provider">The setting provider.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <param name="min">The minimum value of the setting.</param>
    /// <param name="max">The maximum value of the setting.</param>
    /// <param name="percentage">Whether the value is a percentage and should be displayed as such.</param>
    /// <param name="step">The step size of the slider.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateFloatRangeSetting(ISettingsProvider provider, string name,
        (Func<float> get, Action<float> set) accessors,
        float min = float.MinValue, float max = float.MaxValue,
        bool percentage = false, float? step = null)
    {
        return new FloatRangeSetting(name, min, max, percentage, step, accessors.get, accessors.set)
        {
            Provider = provider
        };
    }

    /// <summary>
    ///     Create a quality setting.
    /// </summary>
    /// <param name="provider">The setting provider.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateQualitySetting(ISettingsProvider provider, string name,
        (Func<Quality> get, Action<Quality> set) accessors)
    {
        return new QualitySetting(name, accessors.get, accessors.set)
        {
            Provider = provider
        };
    }

    /// <summary>
    ///     Create a setting for a boolean value.
    /// </summary>
    /// <param name="provider">The setting provider.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <returns>The created setting.</returns>
    public static Setting CreateBooleanSetting(ISettingsProvider provider, string name,
        (Func<bool> get, Action<bool> set) accessors)
    {
        return new BooleanSetting(name, accessors.get, accessors.set)
        {
            Provider = provider
        };
    }

    /// <summary>
    ///     Create a setting for a size value.
    /// </summary>
    /// <param name="provider">The setting provider.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="accessors">Functions to get and set the value.</param>
    /// <param name="current">
    ///     Function to get the current value used by the application, if it is possible to change it independently of the
    ///     setting.
    /// </param>
    /// <returns>The created setting.</returns>
    public static Setting CreateSizeSetting(ISettingsProvider provider, string name,
        (Func<Vector2i> get, Action<Vector2i> set) accessors, Func<Vector2i>? current = null)
    {
        return new SizeSetting(name, accessors.get, accessors.set, current)
        {
            Provider = provider
        };
    }
}
