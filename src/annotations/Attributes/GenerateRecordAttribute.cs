// <copyright file="GenerateRecordAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations.Attributes;

/// <summary>
///     Marks an interface so a record implementation is generated.
///     Optionally accepts a base type. If the type is generic with a single type parameter,
///     the generated record type will be substituted as the type argument.
///     If the type is non-generic, it is implemented as-is. The parameter may be omitted.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class GenerateRecordAttribute : Attribute
{
    /// <summary>
    ///     Initializes the attribute without a base type.
    /// </summary>
    public GenerateRecordAttribute() { }

    /// <summary>
    ///     Initializes the attribute with a base type.
    ///     If the type has exactly one generic parameter, the generated record will be supplied as the argument.
    /// </summary>
    public GenerateRecordAttribute(Type baseType)
    {
        BaseType = baseType;
    }

    /// <summary>
    ///     Optional base type to implement in addition to the marked interface.
    ///     Can be a non-generic type or a generic type with exactly one type parameter.
    /// </summary>
    public Type? BaseType { get; }
}
