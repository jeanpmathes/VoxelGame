// <copyright file="UnitHeader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Serialization;

/// <summary>
///     A header describing information about the serialization system and the unit format.
///     It is used once at the beginning of a serialized unit.
/// </summary>
public class UnitHeader : IValue
{
    private readonly string signature;
    private MetaVersion version = MetaVersion.Current;

    /// <summary>
    ///     Create a new meta header.
    /// </summary>
    /// <param name="signature">The signature of the specific format.</param>
    public UnitHeader(string signature)
    {
        this.signature = signature;
    }

    /// <summary>
    ///     Get the version of the serialization system.
    /// </summary>
    public MetaVersion Version => version;

    /// <inheritdoc />
    public void Serialize(Serializer serializer)
    {
        serializer.Signature(signature);
        serializer.Serialize(ref version);

        if (version > MetaVersion.Current) serializer.Fail("Unit was created with a newer version of the serialization system.");
    }
}
