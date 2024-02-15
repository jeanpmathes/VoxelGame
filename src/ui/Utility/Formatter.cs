// <copyright file="Formatter.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Globalization;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.UI.Utility;

/// <summary>
///     Utilities to format data for display.
/// </summary>
public static class Formatter
{
    /// <summary>
    ///     Format a date time to display both date and time.
    /// </summary>
    /// <param name="dateTime">The date time to format.</param>
    public static string FormatDateTime(DateTime dateTime)
    {
        DateTime localTime = dateTime.ToLocalTime();

        return $"{localTime.ToLongDateString()} - {localTime.ToLongTimeString()}";
    }

    /// <summary>
    ///     Format the time sine an event occurred.
    /// </summary>
    /// <param name="dateTime">The date and time of the event.</param>
    /// <param name="hasOccurred">Whether the event has actually occurred.</param>
    /// <returns>The formatted time since the event occurred.</returns>
    public static string FormatTimeSinceEvent(DateTime? dateTime, out bool hasOccurred)
    {
        hasOccurred = dateTime.HasValue;

        if (!hasOccurred) return Language.TimeSinceEventNever;

        TimeSpan timeSince = DateTime.UtcNow - dateTime!.Value;
        hasOccurred = timeSince > TimeSpan.Zero;

        if (!hasOccurred) return Language.TimeSinceEventNever;

        return timeSince switch
        {
            {TotalDays: > 365 * 5} => Language.TimeSinceEventEternity,
            {TotalDays: > 1} => Format(Language.TimeSinceEventDays, timeSince.TotalDays),
            {TotalHours: > 1} => Format(Language.TimeSinceEventHours, timeSince.TotalHours),
            {TotalMinutes: > 5} => Format(Language.TimeSinceEventMinutes, timeSince.TotalMinutes),
            _ => Language.TimeSinceEventMomentAgo
        };

        string Format(string pattern, double value)
        {
            return string.Format(CultureInfo.CurrentCulture, pattern, (long) value);
        }
    }
}
