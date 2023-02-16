// <copyright file="CreditsMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.RichText;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

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

    internal event EventHandler Cancel = delegate {};

    protected override void CreateMenu(ControlBase menu)
    {
        Button exit = new(menu)
        {
            Text = Language.Back
        };

        exit.Pressed += (_, _) => Cancel(this, EventArgs.Empty);
    }

    protected override void CreateDisplay(ControlBase display)
    {
        TabControl tabs = new(display)
        {
            Dock = Dock.Fill
        };

        DirectoryInfo directory = FileSystem.AccessResourceDirectory("Attribution");

        if (!directory.Exists) return;

        foreach (FileInfo file in directory.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        {
            string name = file.GetFileNameWithoutExtension().Replace(oldChar: '-', newChar: ' ');

            string? text = null;

            try
            {
                text = file.ReadAllText();
            }
            catch (IOException)
            {
                // ignored
            }

            if (text == null) continue;

            Document credits = new();

            Paragraph paragraph = new Paragraph()
                .Font(Context.Fonts.Title).Text(name).LineBreak().LineBreak()
                .Font(Context.Fonts.Default)
                .Text(text).LineBreak();

            credits.Paragraphs.Add(paragraph);

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
