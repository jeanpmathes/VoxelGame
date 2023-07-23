// <copyright file="Arguments.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
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
            description: "Whether to log debug messages. Ignored in DEBUG builds.",
            getDefaultValue: () => false
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

        ILogger GetLogger(InvocationContext context)
        {
            Debug.Assert(logDebugOption != null);

            LoggingParameters parameters = new(context.ParseResult.GetValueForOption(logDebugOption));
            ApplyDebugModification(ref parameters);

            return setupLogging(parameters);
        }

        command.SetHandler(context =>
        {
            context.ExitCode = RunApplication(GetLogger(context),
                logger =>
                {
                    GameParameters gameParameters = new(
                        context.ParseResult.GetValueForOption(loadWorldDirectlyOption),
                        context.ParseResult.GetValueForOption(supportGraphicalDebuggerOption));

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

    [Conditional("DEBUG")]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    private static void ApplyDebugModification(ref LoggingParameters parameters)
    {
        parameters = new LoggingParameters(LogDebug: true);
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
public record GameParameters(int LoadWorldDirectly, bool SupportPIX)
{
    /// <summary>
    ///     Gets the index of the world to load directly, or null if no world should be loaded directly.
    /// </summary>
    public int? DirectlyLoadedWorldIndex => LoadWorldDirectly == 0 ? null : LoadWorldDirectly - 1;
}
