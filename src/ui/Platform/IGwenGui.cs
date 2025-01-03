// <copyright file="IGwenGui.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using Gwen.Net.Control;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.UI.Platform;

/// <summary>
///     Base interface for Gwen GUIs.
/// </summary>
public interface IGwenGui : IResource
{
    /// <summary>
    ///     The root control.
    /// </summary>
    ControlBase Root { get; }

    /// <summary>
    ///     Loads the GUI.
    /// </summary>
    void Load();

    /// <summary>
    ///     Call on window resize.
    /// </summary>
    /// <param name="newSize">The new window size.</param>
    void Resize(Vector2i newSize);

    /// <summary>
    ///     Updates the GUI.
    /// </summary>
    void Update();

    /// <summary>
    ///     Renders the GUI.
    /// </summary>
    void Render();
}
