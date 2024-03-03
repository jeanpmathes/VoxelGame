// <copyright file="FileSystemPath.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A path property.
/// </summary>
public class FileSystemPath : Property
{
    /// <summary>
    ///     Create a new <see cref="FileSystemPath" />.
    /// </summary>
    public FileSystemPath(string name, FileSystemInfo path) : base(name)
    {
        Path = path;
    }

    /// <summary>
    ///     Get the path.
    /// </summary>
    public FileSystemInfo Path { get; }

    /// <exclude />
    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}
