// <copyright file="MainUserInterfaceResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.UI.Resources;

/// <summary>
///     The main resources for the user interface.
///     If these are not loaded, displaying a GUI is not possible.
/// </summary>
public class UserInterfaceRequirements : ResourceCatalog
{
    /// <summary>
    ///     Creates a new instance of the UI resource catalog.
    /// </summary>
    public UserInterfaceRequirements() : base([
        new VGuiLoader(),
        new FontLoader()
    ]) {}
}
