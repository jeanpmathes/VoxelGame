﻿// <copyright file="Formatter.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Globalization;
using Gwen.Net;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.UI.Utilities;

/// <summary>
///     Utilities to format data for display.
/// </summary>
public static class Texts
{
    /// <summary>
    ///     Format a date time to display both date and time.
    /// </summary>
    /// <param name="dateTime">The date time to format.</param>
    public static String FormatDateTime(DateTime dateTime)
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
    public static String FormatTimeSinceEvent(DateTime? dateTime, out Boolean hasOccurred)
    {
        hasOccurred = dateTime.HasValue;

        if (!hasOccurred) return Language.TimeSinceEventNever;

        TimeSpan timeSince = DateTime.UtcNow - dateTime!.Value;
        hasOccurred = timeSince > TimeSpan.Zero;

        if (!hasOccurred) return Language.TimeSinceEventNever;

        return timeSince switch
        {
            {TotalDays: > 365 * 5} => Language.TimeSinceEventEternity,
            {TotalDays: > 7} => Format(Language.TimeSinceEventWeeks, timeSince.TotalDays / 7),
            {TotalDays: > 1} => Format(Language.TimeSinceEventDays, timeSince.TotalDays),
            {TotalHours: > 1} => Format(Language.TimeSinceEventHours, timeSince.TotalHours),
            {TotalMinutes: > 5} => Format(Language.TimeSinceEventMinutes, timeSince.TotalMinutes),
            _ => Language.TimeSinceEventMomentAgo
        };

        String Format(String pattern, Double value)
        {
            return String.Format(CultureInfo.CurrentCulture, pattern, (Int64) value);
        }
    }

    /// <summary>
    ///     Format the status of an operation.
    /// </summary>
    private static String FormatStatus(Status status)
    {
        return status switch
        {
            Status.Ok => Language.OperationStatusOk,
            Status.Running => Language.OperationStatusRunning,
            Status.ErrorOrCancel => Language.OperationStatusError,
            _ => throw Exceptions.UnsupportedEnumValue(status)
        };
    }

    /// <summary>
    ///     Format an operation and its status.
    /// </summary>
    public static String FormatWithStatus(String operation, Status status)
    {
        return $"{operation}: {FormatStatus(status)}";
    }

    /// <summary>
    ///     Get the color for a status.
    /// </summary>
    public static Color GetStatusColor(Status status)
    {
        return status switch
        {
            Status.Ok => Colors.Secondary,
            Status.Running => Colors.Secondary,
            Status.ErrorOrCancel => Colors.Error,
            _ => throw Exceptions.UnsupportedEnumValue(status)
        };
    }
}
