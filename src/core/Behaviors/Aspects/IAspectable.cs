// <copyright file="IAspectable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
/// Objects that can define and contribute to aspects.
/// </summary>
public interface IAspectable
{
    /// <summary>
    /// Invoked during resource validation.
    /// </summary>
    public event EventHandler<ValidationEventArgs> Validation;
    
    /// <summary>
    /// The resource validation event arguments.
    /// </summary>
    public class ValidationEventArgs : EventArgs
    {
        /// <summary>
        /// The validator to use.
        /// </summary>
        public required IValidator Validator { get; init; }
    }
}
