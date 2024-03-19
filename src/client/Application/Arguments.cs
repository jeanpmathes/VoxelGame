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
using VoxelGame.Logging;

namespace VoxelGame.Client.Application;

/// <summary>
///     Supports command line argument handling for this application.
/// </summary>
public static class Arguments
{
    /// <summary>
    ///     Handles the command line arguments.
    /// </summary>
    public static int Handle(string[] args, SetupLogging setupLogging, RunGame runGame)
    {
        RootCommand command = new("Run VoxelGame.");

        var logDebugOption = new Option<bool>(
            "--log-debug",
            description: "Whether to log debug messages. Is enabled by default in DEBUG builds.",
            getDefaultValue: () => Program.IsDebug
        );

        logDebugOption.AddAlias("-dbg");
        command.AddOption(logDebugOption);

        var loadWorldDirectlyOption = new Option<int>(
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

        var supportGraphicalDebuggerOption = new Option<bool>(
            "--pix",
            description: "Whether to configure some features in a way that improve the PIX debugging experience at the cost of performance and validation.",
            getDefaultValue: () => false
        );

        command.AddOption(supportGraphicalDebuggerOption);

        var useGraphicsProcessingUnitBasedValidationOption = new Option<bool>(
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

        ILogger GetLogger(InvocationContext context)
        {
            Debug.Assert(logDebugOption != null);

            return setupLogging(new LoggingParameters(context.ParseResult.GetValueForOption(logDebugOption)));
        }

        command.SetHandler(context =>
        {
            context.ExitCode = RunApplication(GetLogger(context),
                logger =>
                {
                    GameParameters gameParameters = new(
                        context.ParseResult.GetValueForOption(loadWorldDirectlyOption),
                        context.ParseResult.GetValueForOption(enableProfilingOption),
                        context.ParseResult.GetValueForOption(supportGraphicalDebuggerOption),
                        context.ParseResult.GetValueForOption(useGraphicsProcessingUnitBasedValidationOption));

                    return runGame(gameParameters, logger);
                });
        });

        return command.Invoke(args);
    }

    private static int RunApplication(ILogger logger, Func<ILogger, int> app)
    {
        int exitCode = app(logger);

        logger.LogInformation(Events.ApplicationState, "Exiting with code: {ExitCode}", exitCode);

        return exitCode;
    }
}

/// <summary>
///     Sets up logging.
/// </summary>
public delegate ILogger SetupLogging(LoggingParameters parameters);

/// <summary>
///     The parameters for setting up logging.
/// </summary>
public record LoggingParameters(bool LogDebug);

/// <summary>
///     Runs the game.
/// </summary>
public delegate int RunGame(GameParameters parameters, ILogger logger);

/// <summary>
///     The parameters for launching the game.
/// </summary>
public record GameParameters(int LoadWorldDirectly, ProfilerConfiguration Profile, bool SupportPIX, bool UseGBV)
{
    /// <summary>
    ///     Gets the index of the world to load directly, or null if no world should be loaded directly.
    /// </summary>
    public int? DirectlyLoadedWorldIndex => LoadWorldDirectly == 0 ? null : LoadWorldDirectly - 1;
}
