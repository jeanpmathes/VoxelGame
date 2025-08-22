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
    internal static void EmitManual(Client client)
    {
        GenerateManual(client);
    }

    [Conditional("MANUAL")]
    private static void GenerateManual(Client client)
    {
        const String path = "./../../../../../../stp/Resources/Manual";
        DirectoryInfo directory = FileSystem.GetFullPath(path);

        LogGeneratingManual(logger);

        Documentation documentation = new(typeof(Core.App.Application).Assembly);

        Includable controls = new("controls", directory);

        controls.CreateSections(
            client.Keybinds.Binds,
            keybind => Section.Create(keybind.Name, section => section.Text("The key is bound to").Key(keybind.Default).Text("per default.")));

        controls.Generate();

        Includable blocks = new("blocks", directory);
        
        // todo: adapt the manual generation to the new block system
        // todo: it should display all the block properties
        // todo: it should display all block behaviors, with the associated attributes
        // todo: it should display the total number of block states
        // todo: it should do grouping by convention

        blocks.CreateSections(
            Blocks.Instance.GetDocumentedValues<Block>(documentation),
            ((Block block, String description) s) =>
                Section.Create(s.block.Name,
                    section =>
                        section.Text(s.description).NewLine()
                            .List(list => list
                                .Item("ID:").Text(s.block.NamedID, TextStyle.Monospace)
                                .Item("Solid:").Boolean(s.block.IsSolid)
                                .Item("Interactions:").Boolean(s.block.IsInteractable))));

        blocks.Generate();

        Includable fluids = new("fluids", directory);

        fluids.CreateSections(
            Fluids.Instance.GetDocumentedValues<Fluid>(documentation),
            ((Fluid fluid, String description) s) =>
                Section.Create(s.fluid.Name,
                    section => section
                        .Text(s.description).NewLine()
                        .List(list => list
                            .Item("ID:").Text(s.fluid.NamedID, TextStyle.Monospace)
                            .Item("Viscosity:").Text(s.fluid.Viscosity.ToString(CultureInfo.InvariantCulture))
                            .Item("Density:").Text(s.fluid.Density.ToString(CultureInfo.InvariantCulture)))));

        fluids.Generate();

        LogSavedManual(logger, directory.FullName);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger(nameof(ManualBuilder));

    [LoggerMessage(EventId = LogID.ManualBuilder + 0, Level = LogLevel.Information, Message = "Generating game manual")]
    private static partial void LogGeneratingManual(ILogger logger);

    [LoggerMessage(EventId = LogID.ManualBuilder + 1, Level = LogLevel.Information, Message = "Saved game manual to {Path}")]
    private static partial void LogSavedManual(ILogger logger, String path);

    #endregion LOGGING
}
