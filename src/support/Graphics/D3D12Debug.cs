// <copyright file="D3D12Debug.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;

namespace VoxelGame.Support.Graphics;

/// <summary>
///     Offers support for DirectX debug message formatting.
/// </summary>
internal class D3D12Debug
{
    private const String DebugCategory = "DirectX Debug";
    private static readonly ILogger logger = LoggingHelper.CreateLogger<D3D12Debug>();

    private static (D3D12Debug debug, Client client)? instance;

#pragma warning disable S1450

    // Has to be a member to prevent garbage collection.
    private readonly Definition.Native.D3D12MessageFunc debugCallbackDelegate;

#pragma warning restore S1450

    private D3D12Debug(Definition.Native.D3D12MessageFunc debugCallbackDelegate)
    {
        this.debugCallbackDelegate = debugCallbackDelegate;
    }

    /// <summary>
    ///     Enable the debugging features. This method should be called exactly once, and the result must be passed to the
    ///     native configuration.
    /// </summary>
    internal static Definition.Native.D3D12MessageFunc Enable(Client client)
    {
        Debug.Assert(instance is null);

        D3D12Debug debug = new(DebugCallback);
        instance = (debug, client);

        return debug.debugCallbackDelegate;
    }

    private static void DebugCallback(
        Definition.Native.D3D12_MESSAGE_CATEGORY category,
        Definition.Native.D3D12_MESSAGE_SEVERITY severity,
        Definition.Native.D3D12_MESSAGE_ID id,
        String? message, IntPtr context)
    {
        LogLevel level = GetLevel(severity);
        String categoryName = ResolveCategory(category);
        (String idResolved, Int32 eventId) = ResolveEvent(id);

        if (logger.IsEnabled(level))
            // Logging intentionally not trough source generator to allow easily setting level and event id.
            logger.Log(
                level,
                eventId,
                "DirectX Debug | Category: {Category} | Id: {Id} | Message: {Message}",
                categoryName,
                idResolved,
                message);

        Debugger.Log((Int32) level, DebugCategory, $"Category: {categoryName} | Id: {idResolved} | Message: {message}");

        if (id
            is Definition.Native.D3D12_MESSAGE_ID.D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_AT_FAULT
            or Definition.Native.D3D12_MESSAGE_ID.D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_POSSIBLY_AT_FAULT
            or Definition.Native.D3D12_MESSAGE_ID.D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_NOT_AT_FAULT)
        {
            Client client = instance!.Value.client;

            OS.Show("DRED", client.GetDRED());
            OS.Show("Allocator", client.GetAllocatorStatistics());

            Debugger.Break();

            throw new InvalidOperationException("Device removed.");
        }

        if (level >= LogLevel.Warning) Debugger.Break();
    }

    private static LogLevel GetLevel(Definition.Native.D3D12_MESSAGE_SEVERITY severity)
    {
        return severity switch
        {
            Definition.Native.D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_CORRUPTION => LogLevel.Critical,
            Definition.Native.D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_ERROR => LogLevel.Error,
            Definition.Native.D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_WARNING => LogLevel.Warning,
            Definition.Native.D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_INFO => LogLevel.Information,
            Definition.Native.D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_MESSAGE => LogLevel.Debug,
            _ => throw new InvalidOperationException($"Unknown D3D12_MESSAGE_SEVERITY: {severity}")
        };
    }

    private static String ResolveCategory(Definition.Native.D3D12_MESSAGE_CATEGORY category)
    {
        return category switch
        {
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_APPLICATION_DEFINED => "APPLICATION DEFINED",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_MISCELLANEOUS => "MISCELLANEOUS",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_INITIALIZATION => "INITIALIZATION",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_CLEANUP => "CLEANUP",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_COMPILATION => "COMPILATION",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_STATE_CREATION => "STATE CREATION",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_STATE_SETTING => "STATE SETTING",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_STATE_GETTING => "STATE GETTING",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_RESOURCE_MANIPULATION => "MANIPULATION",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_EXECUTION => "EXECUTION",
            Definition.Native.D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_SHADER => "SHADER",
            _ => throw new InvalidOperationException($"Unknown D3D12_MESSAGE_CATEGORY: {category}")
        };
    }

    private static (String, Int32) ResolveEvent(Definition.Native.D3D12_MESSAGE_ID id)
    {
        return (id.ToString(), Events.DirectXDebug);
    }
}
