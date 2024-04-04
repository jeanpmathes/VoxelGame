// <copyright file="BinaryDeserializer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using System.Text;

namespace VoxelGame.Core.Serialization;

/// <summary>
/// </summary>
public sealed class BinaryDeserializer : Serializer, IDisposable
{
    private readonly BinaryReader reader;
    private readonly FileInfo? source;

    /// <summary>
    ///     Create a new binary deserializer.
    ///     Will read a new unit.
    /// </summary>
    /// <param name="stream">The stream to read from. Will close the stream when disposed.</param>
    /// <param name="signature">The signature of the specific format.</param>
    /// <param name="file">The file that is being read from, if any.</param>
    public BinaryDeserializer(Stream stream, String signature, FileInfo? file = null)
    {
        reader = new BinaryReader(stream, Encoding.UTF8);
        source = file;

        Unit = new UnitHeader(signature);
        Unit.Serialize(this);
    }

    /// <inheritdoc />
    protected override UnitHeader Unit { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        reader.Dispose();
    }

    /// <inheritdoc />
    public override void SerializeSmall(ref Int32 value)
    {
        value = reader.Read7BitEncodedInt();
    }

    /// <inheritdoc />
    public override void SerializeSmall(ref Int64 value)
    {
        value = reader.Read7BitEncodedInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref Int32 value)
    {
        value = reader.ReadInt32();
    }

    /// <inheritdoc />
    public override void Serialize(ref UInt32 value)
    {
        value = reader.ReadUInt32();
    }

    /// <inheritdoc />
    public override void Serialize(ref Int64 value)
    {
        value = reader.ReadInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref UInt64 value)
    {
        value = reader.ReadUInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref Int16 value)
    {
        value = reader.ReadInt16();
    }

    /// <inheritdoc />
    public override void Serialize(ref UInt16 value)
    {
        value = reader.ReadUInt16();
    }

    /// <inheritdoc />
    public override void Serialize(ref Byte value)
    {
        value = reader.ReadByte();
    }

    /// <inheritdoc />
    public override void Serialize(ref SByte value)
    {
        value = reader.ReadSByte();
    }

    /// <inheritdoc />
    public override void Serialize(ref Single value)
    {
        value = reader.ReadSingle();
    }

    /// <inheritdoc />
    public override void Serialize(ref Double value)
    {
        value = reader.ReadDouble();
    }

    /// <inheritdoc />
    public override void Serialize(ref Boolean value)
    {
        value = reader.ReadBoolean();
    }

    /// <inheritdoc />
    public override void Serialize(ref Char value)
    {
        value = reader.ReadChar();
    }

    /// <inheritdoc />
    public override void Serialize(ref String value)
    {
        value = reader.ReadString();
    }

    /// <inheritdoc />
    protected override void Serialize(Span<Byte> value)
    {
        var read = 0;

        while (read < value.Length)
        {
            Int32 additional = reader.Read(value[read..]);

            if (additional == 0)
                break;

            read += additional;
        }

        if (read != value.Length)
            Fail("Failed to read the expected amount of bytes.");
    }

    /// <inheritdoc />
    public override void Fail(String message)
    {
        throw new FileFormatException(source?.FullName ?? "<unknown>", message);
    }
}
