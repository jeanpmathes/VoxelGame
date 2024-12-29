// <copyright file="CommandLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Globalization;
using VoxelGame.Client.Console.Commands;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Client.Console;

/// <summary>
/// Loads all commands, creating a <see cref="CommandInvoker"/>.
/// </summary>
public sealed class CommandLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        CommandInvoker invoker = new(context);

        invoker.AddParser(Parser.BuildParser(_ => true, s => s));

        invoker.AddParser(
            Parser.BuildParser(
                s => Int32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => Int32.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(
            Parser.BuildParser(
                s => UInt32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => UInt32.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(
            Parser.BuildParser(
                s => Double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => Double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(Parser.BuildParser(
            s => Enum.IsDefined(typeof(Orientation), s),
            Enum.Parse<Orientation>));

        invoker.AddParser(Parser.BuildParser(s => Boolean.TryParse(s, out _), Boolean.Parse));

        invoker.SearchCommands();
        invoker.AddCommand(new Help(invoker));

        return [invoker];
    }
}
