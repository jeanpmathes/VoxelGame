// <copyright file="StandardMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls;

/// <summary>
///     An abstract menu that can be used to create menus with a standard layout.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal abstract class StandardMenu : ControlBase
{
    private protected StandardMenu(ControlBase parent, Context context) : base(parent)
    {
        Context = context;
        Dock = Dock.Fill;
    }

    internal Context Context { get; }

    protected FontHolder Fonts => Context.Fonts;

    protected void CreateContent()
    {
        Dock = Dock.Fill;

        GridLayout grid = new(this);
        grid.SetColumnWidths(0.3f, 0.7f);
        grid.SetRowHeights(1.0f);

        GridLayout bar = new(grid)
        {
            Dock = Dock.Fill,
            Margin = Margin.Five
        };

        MakeFiller(bar);
        VerticalLayout title = new(bar);
        CreateTitle(title);

        MakeFiller(bar);
        VerticalLayout menu = new(bar);
        CreateMenu(menu);
        MakeFiller(bar);

        bar.SetColumnWidths(1.0f);
        bar.SetRowHeights(0.05f, 0.15f, 0.55f, 0.20f, 0.05f);

        CreateDisplay(grid);
    }

    private static void MakeFiller(ControlBase control)
    {
        VerticalLayout filler = new(control);

        Control.Used(filler);
    }

    private void CreateTitle(ControlBase bar)
    {
        Label title = new(bar)
        {
            Text = Language.VoxelGame,
            Font = Fonts.Title,
            Alignment = Alignment.Center
        };

        Control.Used(title);

        Label subtitle = new(bar)
        {
            Text = ApplicationInformation.Instance.Version,
            Font = Fonts.Subtitle,
            Alignment = Alignment.Center
        };

        Control.Used(subtitle);
    }

    protected abstract void CreateMenu(ControlBase menu);

    protected abstract void CreateDisplay(ControlBase display);
}
