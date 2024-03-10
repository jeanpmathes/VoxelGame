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
    public BinaryDeserializer(Stream stream, string signature, FileInfo? file = null)
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
    public override void SerializeSmall(ref int value)
    {
        value = reader.Read7BitEncodedInt();
    }

    /// <inheritdoc />
    public override void SerializeSmall(ref long value)
    {
        value = reader.Read7BitEncodedInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref int value)
    {
        value = reader.ReadInt32();
    }

    /// <inheritdoc />
    public override void Serialize(ref uint value)
    {
        value = reader.ReadUInt32();
    }

    /// <inheritdoc />
    public override void Serialize(ref long value)
    {
        value = reader.ReadInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref ulong value)
    {
        value = reader.ReadUInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref short value)
    {
        value = reader.ReadInt16();
    }

    /// <inheritdoc />
    public override void Serialize(ref ushort value)
    {
        value = reader.ReadUInt16();
    }

    /// <inheritdoc />
    public override void Serialize(ref byte value)
    {
        value = reader.ReadByte();
    }

    /// <inheritdoc />
    public override void Serialize(ref sbyte value)
    {
        value = reader.ReadSByte();
    }

    /// <inheritdoc />
    public override void Serialize(ref float value)
    {
        value = reader.ReadSingle();
    }

    /// <inheritdoc />
    public override void Serialize(ref double value)
    {
        value = reader.ReadDouble();
    }

    /// <inheritdoc />
    public override void Serialize(ref bool value)
    {
        value = reader.ReadBoolean();
    }

    /// <inheritdoc />
    public override void Serialize(ref char value)
    {
        value = reader.ReadChar();
    }

    /// <inheritdoc />
    public override void Serialize(ref string value)
    {
        value = reader.ReadString();
    }

    /// <inheritdoc />
    protected override void Serialize(Span<byte> value)
    {
        int read = reader.Read(value);

        if (read != value.Length) Fail("Failed to read the expected amount of bytes.");
    }

    /// <inheritdoc />
    public override void Fail(string message)
    {
        if (source != null) throw new FileFormatException(source.FullName, message);

        throw new IOException(message);
    }
}
