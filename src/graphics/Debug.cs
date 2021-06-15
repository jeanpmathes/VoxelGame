// <copyright file="Debug.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Logging;

namespace VoxelGame.Graphics
{
    public class Debug
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<Debug>();

#pragma warning disable S1450

        // Has to be a member to prevent garbage collection.
        private DebugProc debugCallbackDelegate;

#pragma warning restore S1450

        public void Enable()
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.Multisample);

            debugCallbackDelegate = DebugCallback;
            GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);
        }

        private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (id == 131169 || id == 131185 || id == 131218 || id == 131204) return;

            string sourceShort = "NONE";
            switch (source)
            {
                case DebugSource.DebugSourceApi:
                    sourceShort = "API";
                    break;

                case DebugSource.DebugSourceApplication:
                    sourceShort = "APPLICATION";
                    break;

                case DebugSource.DebugSourceOther:
                    sourceShort = "OTHER";
                    break;

                case DebugSource.DebugSourceShaderCompiler:
                    sourceShort = "SHADER COMPILER";
                    break;

                case DebugSource.DebugSourceThirdParty:
                    sourceShort = "THIRD PARTY";
                    break;

                case DebugSource.DebugSourceWindowSystem:
                    sourceShort = "WINDOWS SYSTEM";
                    break;
            }

            string typeShort = "NONE";
            switch (type)
            {
                case DebugType.DebugTypeDeprecatedBehavior:
                    typeShort = "DEPRECATED BEHAVIOR";
                    break;

                case DebugType.DebugTypeError:
                    typeShort = "ERROR";
                    break;

                case DebugType.DebugTypeMarker:
                    typeShort = "MARKER";
                    break;

                case DebugType.DebugTypeOther:
                    typeShort = "OTHER";
                    break;

                case DebugType.DebugTypePerformance:
                    typeShort = "PERFORMANCE";
                    break;

                case DebugType.DebugTypePopGroup:
                    typeShort = "POP GROUP";
                    break;

                case DebugType.DebugTypePortability:
                    typeShort = "PORTABILITY";
                    break;

                case DebugType.DebugTypePushGroup:
                    typeShort = "PUSH GROUP";
                    break;

                case DebugType.DebugTypeUndefinedBehavior:
                    typeShort = "UNDEFINED BEHAVIOR";
                    break;
            }

            string idResolved = "-";
            int eventId = 0;
            switch (id)
            {
                case 0x500:
                    idResolved = "GL_INVALID_ENUM";
                    eventId = Events.GlInvalidEnum;
                    break;

                case 0x501:
                    idResolved = "GL_INVALID_VALUE";
                    eventId = Events.GlInvalidValue;
                    break;

                case 0x502:
                    idResolved = "GL_INVALID_OPERATION";
                    eventId = Events.GlInvalidOperation;
                    break;

                case 0x503:
                    idResolved = "GL_STACK_OVERFLOW";
                    eventId = Events.GlStackOverflow;
                    break;

                case 0x504:
                    idResolved = "GL_STACK_UNDERFLOW";
                    eventId = Events.GlStackUnderflow;
                    break;

                case 0x505:
                    idResolved = "GL_OUT_OF_MEMORY";
                    eventId = Events.GlOutOfMemory;
                    break;

                case 0x506:
                    idResolved = "GL_INVALID_FRAMEBUFFER_OPERATION";
                    eventId = Events.GlInvalidFramebufferOperation;
                    break;

                case 0x507:
                    idResolved = "GL_CONTEXT_LOST";
                    eventId = Events.GlContextLost;
                    break;
            }

            switch (severity)
            {
                case DebugSeverity.DebugSeverityNotification:
                    Logger.LogInformation(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityLow:
                    Logger.LogWarning(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityMedium:
                    Logger.LogError(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityHigh:
                    Logger.LogCritical(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                default:
                    Logger.LogInformation(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;
            }
        }
    }
}