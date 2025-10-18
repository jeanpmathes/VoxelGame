// <copyright file="ConstructibleAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations;

/// <summary>
///     Marks a constructor so a constructible implementation is generated.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class ConstructibleAttribute : Attribute
{
}
