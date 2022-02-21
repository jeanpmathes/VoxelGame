// <copyright file="SettingChangedArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

// XML-Documentation for Records seems to not really work...

#pragma warning disable CS1591
#pragma warning disable CS1572
#pragma warning disable CS1573

namespace VoxelGame.Client.Application
{
    /// <summary>
    ///     Arguments passed to a setting changed event.
    /// </summary>
    /// <param name="OldValue">The old value of the setting.</param>
    /// <param name="NewValue">The new value of the setting.</param>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    public record SettingChangedArgs<T>(T OldValue, T NewValue);
}
