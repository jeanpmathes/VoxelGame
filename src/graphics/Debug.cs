// <copyright file="Debug.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Logging;

namespace VoxelGame.Graphics
{
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
            if (id is 131169 or 131185 or 131218 or 131204) return;

            string sourceShort = source switch
            {
                DebugSource.DebugSourceApi => "API",
                DebugSource.DebugSourceApplication => "APPLICATION",
                DebugSource.DebugSourceOther => "OTHER",
                DebugSource.DebugSourceShaderCompiler => "SHADER COMPILER",
                DebugSource.DebugSourceThirdParty => "THIRD PARTY",
                DebugSource.DebugSourceWindowSystem => "WINDOWS SYSTEM",
                _ => "NONE"
            };

            string typeShort = type switch
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

            (string idResolved, int eventId) = id switch
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

            switch (severity)
            {
                case DebugSeverity.DebugSeverityNotification:
                case DebugSeverity.DontCare:
                    logger.LogInformation(
                        eventId,
                        LoggingTemplate,
                        sourceShort,
                        typeShort,
                        idResolved,
                        Marshal.PtrToStringAnsi(message, length));

                    break;

                case DebugSeverity.DebugSeverityLow:
                    logger.LogWarning(
                        eventId,
                        LoggingTemplate,
                        sourceShort,
                        typeShort,
                        idResolved,
                        Marshal.PtrToStringAnsi(message, length));

                    break;

                case DebugSeverity.DebugSeverityMedium:
                    logger.LogError(
                        eventId,
                        LoggingTemplate,
                        sourceShort,
                        typeShort,
                        idResolved,
                        Marshal.PtrToStringAnsi(message, length));

                    break;

                case DebugSeverity.DebugSeverityHigh:
                    logger.LogCritical(
                        eventId,
                        LoggingTemplate,
                        sourceShort,
                        typeShort,
                        idResolved,
                        Marshal.PtrToStringAnsi(message, length));

                    break;

                default:
                    logger.LogInformation(
                        eventId,
                        LoggingTemplate,
                        sourceShort,
                        typeShort,
                        idResolved,
                        Marshal.PtrToStringAnsi(message, length));

                    break;
            }
        }
    }
}