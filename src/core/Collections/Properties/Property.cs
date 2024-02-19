// <copyright file="Property.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     This is the base class for the elements of the property collection.
///     It uses the composite pattern to allow to freely combine data of different types.
/// </summary>
public abstract class Property
{
    /// <summary>
    ///     Creates a new property with the given name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    protected Property(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     The name of the property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Accepts a visitor and calls the appropriate method.
    /// </summary>
    internal abstract void Accept(Visitor visitor);
}
