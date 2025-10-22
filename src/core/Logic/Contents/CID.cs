// <copyright file = "CID.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Contents;

/// <summary>
/// Represents a content identifier. Must be constructed from an unlocalized string.
/// </summary>
public readonly record struct CID(String Identifier)
{
    /// <summary>
    /// Get the resource ID for the content type T with this CID.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <returns>The resource ID.</returns>
    public RID GetResourceID<T>() where T : IContent
    {
        return RID.Named<T>(Identifier);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return Identifier;
    }
}
