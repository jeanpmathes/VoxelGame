// <copyright file="GraphicalUserInterface.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System.Drawing;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Core;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Themes;
using VoxelGame.Presentation.New.Platform.Graphics;
using VoxelGame.Presentation.New.Platform.Input;

namespace VoxelGame.Presentation.New.Platform;

/// <summary>
///     An active graphical user interface (GUI) that is rendered and process input.
///     This class owns the UI trees and streamlines setup and usage.
/// </summary>
public sealed class GraphicalUserInterface : IResource
{
    private readonly Client client;
    private readonly Theme theme;
    private readonly ClientInputSource inputSource;

    private InputBufferAdapter? inputBuffer;

    private GraphicalUserInterface(Client client, Theme theme)
    {
        this.client = client;
        this.theme = theme;

        inputSource = new ClientInputSource(client);
    }

    private Renderer? Renderer { get; set; }

    /// <summary>
    ///     The root control of the GUI.
    /// </summary>
    public Canvas? Root { get; private set; } // todo: maybe make this a Element, not Canvas, or maybe remove completely and add a method to just set the content

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<GraphicalUserInterface>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.GUI;

    /// <inheritdoc />
    public void Dispose()
    {
        Root?.Dispose();
        Renderer?.Dispose();

        inputSource.Dispose();
    }

    /// <summary>
    ///     Create a new graphical user interface.
    /// </summary>
    /// <param name="client">The client that will display this GUI.</param>
    /// <param name="theme">The theme the GUI will use.</param>
    /// <returns></returns>
    public static GraphicalUserInterface Create(Client client, Theme theme)
    {
        return new GraphicalUserInterface(client, theme);
    }

    /// <summary>
    ///     Load the GUI, which must be done exactly once before using it.
    /// </summary>
    /// <param name="size">The initial screen size.</param>
    public void Load(Vector2i size)
    {
        Renderer = new Renderer(client);
        Root = Canvas.Create(Renderer, theme);

        inputBuffer = new InputBufferAdapter(new InputScaleWithCanvasAdapter(Root, new InputForwardToBindingAdapter(Root.Input)));
        inputSource.AddReceiver(inputBuffer);

        Root.SetRenderingSize(new Size(size.X, size.Y));
    }

    /// <summary>
    ///     Render the GUI.
    /// </summary>
    public void Render()
    {
        Root?.Render();
    }

    /// <summary>
    ///     Update the GUI, performing input processing.
    /// </summary>
    public void Update()
    {
        inputBuffer?.Send();
    }

    /// <summary>
    ///     Inform the GUI that the screen has been resized.
    /// </summary>
    /// <param name="size">The new size of the screen.</param>
    public void Resize(Vector2i size)
    {
        Root?.SetRenderingSize(new Size(size.X, size.Y));
    }
}
