// <copyright file="CreditsMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.RichText;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

/// <summary>
///     The menu that shows the credits.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class CreditsMenu : StandardMenu
{
    internal CreditsMenu(ControlBase parent, Context context) : base(parent, context)
    {
        CreateContent();
    }

    internal event EventHandler? Cancel;

    protected override void CreateMenu(ControlBase menu)
    {
        Button exit = new(menu)
        {
            Text = Language.Back
        };

        exit.Released += (_, _) => Cancel?.Invoke(this, EventArgs.Empty);
    }

    protected override void CreateDisplay(ControlBase display)
    {
        TabControl tabs = new(display)
        {
            Dock = Dock.Fill
        };

        foreach ((Document credits, String name) in Context.Resources.CreateAttributions(Context))
        {
            ScrollControl page = new(tabs)
            {
                CanScrollH = false
            };

            RichLabel content = new(page)
            {
                Document = credits
            };

            Control.Used(content);

            tabs.AddPage(name, page);
        }
    }
}
