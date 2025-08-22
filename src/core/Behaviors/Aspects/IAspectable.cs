// <copyright file="IAspectable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;

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
        /// The resource loading context in which the validation is performed.
        /// </summary>
        public required IResourceContext Context { get; init; }
    }
}
