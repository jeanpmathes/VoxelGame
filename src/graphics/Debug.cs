// <copyright file="Debug.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using VoxelGame.Logging;

namespace VoxelGame.Graphics;

/// <summary>
///     Offers support for modern OpenGL debugging.
/// </summary>
public class Debug
{
    private const string LoggingTemplate =
        "OpenGL Debug | Source: {Source} | Type: {Type} | Event: {Event} | Message: {Message}";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Debug>();

#pragma warning disable S1450

    // Has to be a member to prevent garbage collection.
    private DebugProc debugCallbackDelegate = null!;

#pragma warning restore S1450

    /// <summary>
    ///     Enable the debugging features.
    /// </summary>
    public void Enable()
    {
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.Multisample);

        debugCallbackDelegate = DebugCallback;
        GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);
    }

    private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity,
        int length, IntPtr message, IntPtr userParam)
    {
        if (id is 131185) return;

        string sourceName = GetSourceName(source);
        string typeName = GetTypeName(type);
        (string idResolved, int eventId) = ResolveEvent(id);
        LogLevel level = GetLevel(severity);

        logger.Log(
            level,
            eventId,
            LoggingTemplate,
            sourceName,
            typeName,
            idResolved,
            Marshal.PtrToStringAnsi(message, length));

        if (level >= LogLevel.Error) Debugger.Break();
    }

    private static (string, int) ResolveEvent(int id)
    {
        return id switch
        {
            0x500 => ("GL_INVALID_ENUM", Events.GlInvalidEnum),
            0x501 => ("GL_INVALID_ENUM", Events.GlInvalidEnum),
            0x502 => ("GL_INVALID_OPERATION", Events.GlInvalidOperation),
            0x503 => ("GL_STACK_OVERFLOW", Events.GlStackOverflow),
            0x504 => ("GL_STACK_UNDERFLOW", Events.GlStackUnderflow),
            0x505 => ("GL_OUT_OF_MEMORY", Events.GlOutOfMemory),
            0x506 => ("GL_INVALID_FRAMEBUFFER_OPERATION", Events.GlInvalidFramebufferOperation),
            0x507 => ("GL_CONTEXT_LOST", Events.GlContextLost),
            _ => ("-", 0)
        };
    }

    private static string GetTypeName(DebugType type)
    {
        return type switch
        {
            DebugType.DebugTypeDeprecatedBehavior => "DEPRECATED BEHAVIOR",
            DebugType.DebugTypeError => "ERROR",
            DebugType.DebugTypeMarker => "MARKER",
            DebugType.DebugTypeOther => "OTHER",
            DebugType.DebugTypePerformance => "PERFORMANCE",
            DebugType.DebugTypePopGroup => "POP GROUP",
            DebugType.DebugTypePortability => "PORTABILITY",
            DebugType.DebugTypePushGroup => "PUSH GROUP",
            DebugType.DebugTypeUndefinedBehavior => "UNDEFINED BEHAVIOR",
            _ => "NONE"
        };
    }

    private static string GetSourceName(DebugSource source)
    {
        return source switch
        {
            DebugSource.DebugSourceApi => "API",
            DebugSource.DebugSourceApplication => "APPLICATION",
            DebugSource.DebugSourceOther => "OTHER",
            DebugSource.DebugSourceShaderCompiler => "SHADER COMPILER",
            DebugSource.DebugSourceThirdParty => "THIRD PARTY",
            DebugSource.DebugSourceWindowSystem => "WINDOWS SYSTEM",
            _ => "NONE"
        };
    }

    private static LogLevel GetLevel(DebugSeverity severity)
    {
        return severity switch
        {
            DebugSeverity.DebugSeverityNotification => LogLevel.Information,
            DebugSeverity.DontCare => LogLevel.Information,
            DebugSeverity.DebugSeverityLow => LogLevel.Warning,
            DebugSeverity.DebugSeverityMedium => LogLevel.Error,
            DebugSeverity.DebugSeverityHigh => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }
}

