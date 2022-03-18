// <copyright file="ManualBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using System.Globalization;
using VoxelGame.Core;
using VoxelGame.Core.Logic;
using VoxelGame.Manual;
using VoxelGame.Manual.Modifiers;
using VoxelGame.Manual.Utility;
using Section = VoxelGame.Manual.Section;

namespace VoxelGame.Client.Application;

/// <summary>
///     Utility class that allows to build the Manual for the game.
/// </summary>
public static class ManualBuilder
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
        const string path = "./../../../../../../Setup/Resources/Manual";

        Documentation documentation = new(typeof(ApplicationInformation).Assembly);

        Includable controls = new("controls", path);

        controls.CreateSections(
            Client.Instance.Keybinds.Binds,
            keybind => Section.Create(keybind.Name)
                .Text("The key is bound to").Key(keybind.Default).Text("per default.").EndSection());

        controls.Generate();

        Includable blocks = new("blocks", path);

        blocks.CreateSections(
            typeof(Block).GetStaticValues<Block>(documentation),
            ((Block block, string description) s) => Section.Create(s.block.Name)
                .Text(s.description).NewLine()
                .BeginList()
                .Item("ID:").Text(s.block.NamedId, TextStyle.Monospace)
                .Item("Solid:").Boolean(s.block.IsSolid)
                .Item("Interactions:").Boolean(s.block.IsInteractable)
                .Item("Replaceable:").Boolean(s.block.IsReplaceable)
                .Finish().EndSection());

        blocks.Generate();

        Includable liquids = new("liquids", path);

        liquids.CreateSections(
            typeof(Liquid).GetStaticValues<Liquid>(documentation),
            ((Liquid liquid, string description) s) => Section.Create(s.liquid.Name)
                .Text(s.description).NewLine()
                .BeginList()
                .Item("ID:").Text(s.liquid.NamedId, TextStyle.Monospace)
                .Item("Viscosity:").Text(s.liquid.Viscosity.ToString(CultureInfo.InvariantCulture))
                .Item("Density:").Text(s.liquid.Density.ToString(CultureInfo.InvariantCulture))
                .Finish().EndSection());

        liquids.Generate();
    }
}
