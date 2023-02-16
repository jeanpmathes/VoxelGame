// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Input;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     The context in which the user interface is running.
/// </summary>
internal sealed class Context : IDisposable
{
    internal Context(FontHolder fonts, InputListener input, UIResources resources)
    {
        Fonts = fonts;
        Input = input;
        Resources = resources;
    }

    internal FontHolder Fonts { get; }
    internal InputListener Input { get; }

    internal UIResources Resources { get; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing) Fonts.Dispose();
    }

    ~Context()
    {
        Dispose(disposing: false);
    }
}

