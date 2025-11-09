// <copyright file="ComponentSubjectAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations.Attributes;

/// <summary>
///     Marks a class as the subject in a component composition, meaning it can have components.
/// </summary>
/// <param name="componentType">The type of the components managed by the subject.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ComponentSubjectAttribute(Type componentType) : Attribute
{
    /// <summary>
    ///     Gets the component type associated with the subject.
    /// </summary>
    public Type ComponentType { get; } = componentType;
}
