// <copyright file="PerformanceSensitiveAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations.Attributes;

/// <summary>
///     Marks a method, constructor, property, or field as performance sensitive.
///     This indicates that the annotated element is critical for performance and should be optimized accordingly.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public sealed class PerformanceSensitiveAttribute : Attribute;
