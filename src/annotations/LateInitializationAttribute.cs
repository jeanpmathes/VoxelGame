// <copyright file="LateInitializationAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations;

/// <summary>
/// Marks a field to be initialized late, i.e., not in the constructor.
/// This will generate a safe(ish) getter for the field that throws an exception if the field is accessed before being initialized.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class LateInitializationAttribute : Attribute
{
    
}
