// <copyright file="UI.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.UI.Resources;

/// <summary>
///     Resources that are displayed in the user interface as content.
/// </summary>
public class UserInterfaceContent : ResourceCatalog
{
    /// <summary>
    ///     Creates a new instance of the UI resource catalog.
    /// </summary>
    public UserInterfaceContent() : base([
        new AttributionLoader()
    ]) {}
}
