// <copyright file="ManualBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Manual;
using VoxelGame.Manual.Modifiers;
using VoxelGame.Manual.Utility;
using VoxelGame.Toolkit.Utilities;
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

        var categories = Blocks.Instance.Categories.Select(category => new
        {
            Name = Reflections.GetName(category.GetType()),
            Category = category,
            Description = documentation.GetTypeSummary(category.GetType())
        });

        blocks.CreateSections(categories,
            category => Section.Create(category.Name,
                section =>
                {
                    section.Text(category.Description).NewLine();

                    foreach (PropertyInfo contentInfo in Reflections.GetPropertiesOfType<IContent>(category.Category))
                    {
                        var content = (IContent) contentInfo.GetValue(category.Category)!;
                        String contentDescription = documentation.GetPropertySummary(contentInfo);

                        switch (content)
                        {
                            case Block block:
                                section.SubSection(block.Name,
                                    sub =>
                                        AddBlockDetails(sub.Text(contentDescription).NewLine(), block));

                                break;

                            case IConvention convention:
                                section.SubSection(AddSpacesToPascalCase(contentInfo.Name),
                                    subSection =>
                                    {
                                        subSection.Text(contentDescription).NewLine();

                                        foreach (PropertyInfo blockInfo in Reflections.GetPropertiesOfType<Block>(convention))
                                        {
                                            var block = blockInfo.GetValue(content) as Block;
                                            String blockDescription = documentation.GetPropertySummary(blockInfo);

                                            if (block == null)
                                                continue;

                                            AddBlockDetails(subSection.Text($"{block.Name}: {blockDescription}").NewLine(), block).NewLine();
                                        }

                                        return subSection;
                                    });

                                break;
                        }
                    }

                    return section;
                }));

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

    private static Chainable AddBlockDetails(Chainable chain, Block block)
    {
        return chain
            .Table("ll",
                table =>
                {
                    table
                        .Row(row => row
                            .Cell(cell => cell.Text("Named ID"))
                            .Cell(cell => cell.Text(block.ContentID.ToString(), TextStyle.Monospace)))
                        .Row(row => row
                            .Cell(cell => cell.Text("ID"))
                            .Cell(cell => cell.Text(block.BlockID.ToString(CultureInfo.InvariantCulture), TextStyle.Monospace)))
                        .Row(row => row
                            .Cell(cell => cell.Text("State Count"))
                            .Cell(cell => cell.Text(block.States.Count.ToString(CultureInfo.InvariantCulture))));

                    foreach ((String name, Boolean value) in GetProperties(block))
                    {
                        table.Row(row => row
                            .Cell(cell => cell.Text(name))
                            .Cell(cell => cell.Boolean(value)));
                    }
                })
            .NewLine()
            .Text("Behaviors:")
            .NewLine()
            .Table("ll",
                table =>
                {
                    foreach ((String name, IEnumerable<String> attributes) in GetBehaviors(block))
                    {
                        table.Row(row =>
                        {
                            row.Cell(cell => cell.Text(name));

                            List<String> list = attributes.ToList();

                            if (list.Count > 0)
                            {
                                row.Cell(cell =>
                                {
                                    var first = true;

                                    foreach (String attribute in list)
                                    {
                                        if (!first)
                                            cell.Text(", ");

                                        cell.Text(attribute, TextStyle.Monospace).NewLine();

                                        first = false;
                                    }
                                });
                            }
                            else
                            {
                                row.Cell(cell => cell.Text("—"));
                            }
                        });
                    }
                });
    }

    private static IEnumerable<(String, Boolean)> GetProperties(Block block)
    {
        yield return ("Opaque", block.IsOpaque);
        yield return ("Mesh At Non Opaques", block.MeshFaceAtNonOpaques);
        yield return ("Solid", block.IsSolid);
        yield return ("Unshaded", block.IsUnshaded);
        yield return ("Collider", block.IsCollider);
        yield return ("Trigger", block.IsTrigger);
        yield return ("Interactable", block.IsInteractable);
    }

    private static IEnumerable<(String, IEnumerable<String>)> GetBehaviors(Block block)
    {
        Dictionary<String, List<IAttribute>> attributes = [];

        foreach (IScoped scoped in block.States.Entries)
        {
            if (scoped is not Scope scope)
                continue;

            foreach (IScoped entry in scope.Entries)
            {
                if (entry is IAttribute attribute)
                {
                    attributes.GetOrAdd(scoped.Name, []).Add(attribute);
                }
            }
        }

        foreach (BlockBehavior behavior in block.Behaviors)
        {
            String name = Reflections.GetLongName(behavior.GetType());

            yield return (Reflections.GetName(behavior.GetType()), attributes.GetValueOrDefault(name) is {Count: > 0} list
                ? list.Select(attribute => attribute.Name)
                : []);
        }
    }

    private static String AddSpacesToPascalCase(String name)
    {
        return String.Concat(name.Select((x, i) => i > 0 && Char.IsUpper(x) ? $" {x}" : $"{x}"));
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger(nameof(ManualBuilder));

    [LoggerMessage(EventId = LogID.ManualBuilder + 0, Level = LogLevel.Information, Message = "Generating game manual")]
    private static partial void LogGeneratingManual(ILogger logger);

    [LoggerMessage(EventId = LogID.ManualBuilder + 1, Level = LogLevel.Information, Message = "Saved game manual to {Path}")]
    private static partial void LogSavedManual(ILogger logger, String path);

    #endregion LOGGING
}
