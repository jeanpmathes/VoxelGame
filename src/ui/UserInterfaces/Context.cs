// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Input;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     The context in which the user interface is running.
/// </summary>
internal sealed class Context
{
    internal Context(InputListener input, UIResources resources)
    {
        Fonts = resources.Fonts;
        Input = input;
        Resources = resources;
    }

    internal FontHolder Fonts { get; }
    internal InputListener Input { get; }

    internal UIResources Resources { get; }
}
