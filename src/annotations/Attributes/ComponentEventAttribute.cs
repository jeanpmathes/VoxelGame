// <copyright file="ComponentEventAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations.Attributes;

/// <summary>
///     Marks a method on a component subject as being forwarded to its components.
/// </summary>
/// <param name="componentMethodName">The method name that should be invoked on the components.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ComponentEventAttribute(String componentMethodName) : Attribute
{
    /// <summary>
    ///     Gets the component method name that should be invoked, if explicitly specified.
    /// </summary>
    public String? ComponentMethodName { get; } = componentMethodName;
}
