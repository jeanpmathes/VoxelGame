// <copyright file="Convention.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Definitions;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
/// Abstract base class for conventions.
/// </summary>
/// <param name="namedID">The named ID of the convention.</param>
/// <param name="builder">The block builder used to build the blocks in the convention.</param>
/// <typeparam name="TConvention">The concrete type of the convention.</typeparam>
public abstract class Convention<TConvention>(String namedID, BlockBuilder builder) : IConvention where TConvention : IConvention
{
    /// <inheritdoc />
    public String NamedID { get; } = namedID;
    
    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<TConvention>(namedID);
    
    /// <inheritdoc />
    public IEnumerable<IContent> Content => builder.Registry.RetrieveContent();
    
    #region DISPOSABLE

    /// <summary>
    /// Override this method to dispose of object used by the convention.
    /// Note that most of the time, you will not need to override this method.
    /// </summary>
    /// <param name="disposing">Whether the method is called from the Dispose method or from the finalizer.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        // Nothing to dispose by default.
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Convention()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
