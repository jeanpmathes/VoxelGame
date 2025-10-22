// <copyright file="LateInitializationAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations.Attributes;

/// <summary>
///     Marks a property to be initialized late, i.e., not in the constructor.
///     This will generate a safe(ish) getter for the property that throws an exception if the field is accessed before
///     being initialized.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LateInitializationAttribute : Attribute;
