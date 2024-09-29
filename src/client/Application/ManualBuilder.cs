// <copyright file="ManualBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using VoxelGame.Core;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Manual;
using VoxelGame.Manual.Modifiers;
using VoxelGame.Manual.Utility;
using Section = VoxelGame.Manual.Section;

namespace VoxelGame.Client.Application;

/// <summary>
///     Utility class that allows to build the Manual for the game.
/// </summary>
public static partial class ManualBuilder
{
    /// <summary>
    ///     Emit a manual, if the required build flags are set.
    /// </summary>
    public static void EmitManual()
    {
        GenerateManual();
    }

    [Conditional("MANUAL")]
    private static void GenerateManual()
    {
        const String path = "./../../../../../../SetUp/Resources/Manual";
        DirectoryInfo directory = FileSystem.GetFullPath(path);

        LogGeneratingManual(logger);

        Documentation documentation = new(typeof(ApplicationInformation).Assembly);

        Includable controls = new("controls", directory);

        controls.CreateSections(
            Client.Instance.Keybinds.Binds,
            keybind => Section.Create(keybind.Name)
                .Text("The key is bound to").Key(keybind.Default).Text("per default.").EndSection());

        controls.Generate();

        Includable blocks = new("blocks", directory);

        blocks.CreateSections(
            Blocks.Instance.GetValues<Block>(documentation),
            ((Block block, String description) s) => Section.Create(s.block.Name)
                .Text(s.description).NewLine()
                .BeginList()
                .Item("ID:").Text(s.block.NamedID, TextStyle.Monospace)
                .Item("Solid:").Boolean(s.block.IsSolid)
                .Item("Interactions:").Boolean(s.block.IsInteractable)
                .Item("Replaceable:").Boolean(s.block.IsReplaceable)
                .Finish().EndSection());

        blocks.Generate();

        Includable fluids = new("fluids", directory);

        fluids.CreateSections(
            Fluids.Instance.GetValues<Fluid>(documentation),
            ((Fluid fluid, String description) s) => Section.Create(s.fluid.Name)
                .Text(s.description).NewLine()
                .BeginList()
                .Item("ID:").Text(s.fluid.NamedID, TextStyle.Monospace)
                .Item("Viscosity:").Text(s.fluid.Viscosity.ToString(CultureInfo.InvariantCulture))
                .Item("Density:").Text(s.fluid.Density.ToString(CultureInfo.InvariantCulture))
                .Finish().EndSection());

        fluids.Generate();

        LogSavedManual(logger, directory.FullName);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger(nameof(ManualBuilder));

    [LoggerMessage(EventId = Events.ApplicationInformation, Level = LogLevel.Information, Message = "Generating game manual")]
    private static partial void LogGeneratingManual(ILogger logger);

    [LoggerMessage(EventId = Events.ApplicationInformation, Level = LogLevel.Information, Message = "Saved game manual to {Path}")]
    private static partial void LogSavedManual(ILogger logger, String path);

    #endregion LOGGING
}
