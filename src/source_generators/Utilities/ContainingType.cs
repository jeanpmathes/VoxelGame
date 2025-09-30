// <copyright file="ContainingType.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.SourceGenerators.Utilities;

/// <summary>
///     Class to represent the containing type of declarations, able to handle nested types.
/// </summary>
public class ContainingType(String accessbility, String keyword, String name, String? typeParameters, String constraints, ContainingType? child)
{
    /// <summary>
    ///     The child containing type, if any.
    /// </summary>
    public ContainingType? Child { get; } = child;

    /// <summary>
    ///     The accessibility of the containing type (e.g., "public", "internal", "private").
    /// </summary>
    public String Accessibility { get; } = accessbility;

    /// <summary>
    ///     The keyword of the containing type (e.g., "class", "struct", "record", "interface").
    /// </summary>
    public String Keyword { get; } = keyword;

    /// <summary>
    ///     The name of the containing type.
    /// </summary>
    public String Name { get; } = name;

    /// <summary>
    ///     The type parameters of the containing type, if any (including angle brackets).
    /// </summary>
    public String? TypeParameters { get; } = typeParameters;

    /// <summary>
    ///     The constraints of the containing type, if any.
    /// </summary>
    public String Constraints { get; } = constraints;

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is not ContainingType other)
            return false;

        if (obj == this)
            return true;

        return Accessibility == other.Accessibility
               && Keyword == other.Keyword
               && Name == other.Name
               && TypeParameters == other.TypeParameters
               && Constraints == other.Constraints
               && Equals(Child, other.Child);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        unchecked
        {
            Int32 hashCode = Accessibility.GetHashCode();
            hashCode = hashCode * 397 ^ Keyword.GetHashCode();
            hashCode = hashCode * 397 ^ Name.GetHashCode();
            hashCode = hashCode * 397 ^ (TypeParameters != null ? TypeParameters.GetHashCode() : 0);
            hashCode = hashCode * 397 ^ Constraints.GetHashCode();
            hashCode = hashCode * 397 ^ (Child != null ? Child.GetHashCode() : 0);

            return hashCode;
        }
    }
}
