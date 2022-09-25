// <copyright file="CreditsMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using Gwen.Net.RichText;
using VoxelGame.Core.Resources.Language;
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
        Document credits = new();

        Paragraph content = new Paragraph()
            .Font(Context.Fonts.Title).Text("Credits").LineBreak().LineBreak()
            .Font(Context.Fonts.Default)
            .Text("Images").LineBreak()
            .Text(Source.GetTextContent("Resources/GUI/Icons/attribution.txt")).LineBreak()
            .Text("Textures").LineBreak()
            .Text(Source.GetTextContent("Resources/Textures/attribution.txt")).LineBreak()
            .Text("Code").LineBreak()
            .Text("Noise Generation: https://github.com/Auburn/FastNoiseLite").LineBreak()
            .Text("glsl Noise Generation: https://github.com/stegu/webgl-noise").LineBreak()
            .Text("Order Independent Transparency: https://learnopengl.com/Guest-Articles/2020/OIT/Weighted-Blended").LineBreak()
            .Text("OpenTK Tutorials: https://opentk.net/learn/index.html").LineBreak();

        credits.Paragraphs.Add(content);

        RichLabel creditsDisplay = new(display)
        {
            Document = credits
        };
    }
}
