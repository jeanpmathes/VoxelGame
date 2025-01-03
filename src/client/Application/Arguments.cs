// <copyright file="Arguments.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Profiling;

namespace VoxelGame.Client.Application;

/// <summary>
///     Supports command line argument handling for this application.
/// </summary>
public static partial class Arguments
{
    /// <summary>
    ///     Handles the command line arguments.
    /// </summary>
    public static Int32 Handle(String[] args, SetUpLogging setUpLogging, RunGame runGame)
    {
        RootCommand command = new("Run VoxelGame.");

        var logDebugOption = new Option<Boolean>(
            "--log-debug",
            description: "Whether to log debug messages. Is enabled by default in DEBUG builds.",
            getDefaultValue: () => Program.IsDebug
        );

        logDebugOption.AddAlias("-dbg");
        command.AddOption(logDebugOption);

        var loadWorldDirectlyOption = new Option<Int32>(
            "--load-world-directly",
            description: "Select the number of a world to load directly, skipping the main menu. Use 0 to disable.",
            getDefaultValue: () => 0
        );

        loadWorldDirectlyOption.AddAlias("-l");

        loadWorldDirectlyOption.AddValidator(result =>
        {
            if (result.GetValueForOption(loadWorldDirectlyOption) < 0) result.ErrorMessage = "The value must be greater than or equal to 0.";
        });

        command.AddOption(loadWorldDirectlyOption);

        var supportGraphicalDebuggerOption = new Option<Boolean>(
            "--pix",
            description: "Whether to configure some features in a way that improve the PIX debugging experience at the cost of performance and validation.",
            getDefaultValue: () => false
        );

        command.AddOption(supportGraphicalDebuggerOption);

        var useGraphicsProcessingUnitBasedValidationOption = new Option<Boolean>(
            "--gbv",
            description: "Whether to use GPU-based validation. Has no effect if PIX support is enabled.",
            getDefaultValue: () => false
        );

        command.AddOption(useGraphicsProcessingUnitBasedValidationOption);

        var enableProfilingOption = new Option<ProfilerConfiguration>(
            "--profile",
            description: "The profiler configuration to use. In DEBUG builds, basic profiling is used by default. Otherwise, no profiling is done.",
            getDefaultValue: () => Program.IsDebug ? ProfilerConfiguration.Basic : ProfilerConfiguration.Disabled
        );

        enableProfilingOption.AddAlias("-p");
        command.AddOption(enableProfilingOption);

        ILogger? logger = null;
        GameParameters? gameParameters = null;

        command.SetHandler(context =>
        {
            logger = GetLogger(context);

            gameParameters = new GameParameters(
                context.ParseResult.GetValueForOption(loadWorldDirectlyOption),
                context.ParseResult.GetValueForOption(enableProfilingOption),
                context.ParseResult.GetValueForOption(supportGraphicalDebuggerOption),
                context.ParseResult.GetValueForOption(useGraphicsProcessingUnitBasedValidationOption));
        });

        command.Invoke(args);

        if (logger == null || gameParameters == null)
            return 1;

        Int32 exitCode = runGame(gameParameters, logger);

        LogExitingWithCode(logger, exitCode);

        return exitCode;

        ILogger GetLogger(InvocationContext context)
        {
            Debug.Assert(logDebugOption != null);

            return setUpLogging(new LoggingParameters(context.ParseResult.GetValueForOption(logDebugOption)));
        }
    }

    #region LOGGING

    [LoggerMessage(EventId = LogID.Arguments + 0, Level = LogLevel.Information, Message = "Exiting with code: {ExitCode}")]
    private static partial void LogExitingWithCode(ILogger logger, Int32 exitCode);

    #endregion LOGGING
}

/// <summary>
///     Sets up logging.
/// </summary>
public delegate ILogger SetUpLogging(LoggingParameters parameters);

/// <summary>
///     The parameters for setting up logging.
/// </summary>
public record LoggingParameters(Boolean LogDebug);

/// <summary>
///     Runs the game.
/// </summary>
public delegate Int32 RunGame(GameParameters parameters, ILogger logger);

/// <summary>
///     The parameters for launching the game.
/// </summary>
public record GameParameters(Int32 LoadWorldDirectly, ProfilerConfiguration Profile, Boolean SupportPIX, Boolean UseGBV)
{
    /// <summary>
    ///     Gets the index of the world to load directly, or null if no world should be loaded directly.
    /// </summary>
    public Int32? DirectlyLoadedWorldIndex => LoadWorldDirectly == 0 ? null : LoadWorldDirectly - 1;
}
