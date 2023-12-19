// <copyright file="GwenGuiFactory.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using VoxelGame.Support.Core;

namespace VoxelGame.UI.Platform;

/// <summary>
///     The factory for <see cref="IGwenGui" /> instances.
/// </summary>
public static class GwenGuiFactory
{
    /// <summary>
    ///     Creates a new <see cref="IGwenGui" /> instance from a <see cref="Client" />.
    /// </summary>
    public static IGwenGui CreateFromClient(Client window, GwenGuiSettings? settings = default)
    {
        settings ??= GwenGuiSettings.Default;

        return new VGui(window, settings);
    }
}
