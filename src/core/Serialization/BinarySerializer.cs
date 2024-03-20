// <copyright file="BinarySerializer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using System.Text;

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Serializer for binary data.
/// </summary>
public sealed class BinarySerializer : Serializer, IDisposable
{
    private readonly BinaryWriter writer;
    private readonly FileInfo? destination;

    /// <summary>
    ///     Create a new binary serializer.
    ///     Will begin a new unit.
    /// </summary>
    /// <param name="stream">The stream to write to. Will close the stream when disposed.</param>
    /// <param name="signature">The signature of the specific format.</param>
    /// <param name="file">The file that is being written to, if any.</param>
    public BinarySerializer(Stream stream, string signature, FileInfo? file = null)
    {
        writer = new BinaryWriter(stream, Encoding.UTF8);
        destination = file;

        Unit = new UnitHeader(signature);
        Unit.Serialize(this);
    }

    /// <inheritdoc />
    protected override UnitHeader Unit { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        writer.Dispose();
    }

    /// <inheritdoc />
    public override void SerializeSmall(ref int value)
    {
        writer.Write7BitEncodedInt(value);
    }

    /// <inheritdoc />
    public override void SerializeSmall(ref long value)
    {
        writer.Write7BitEncodedInt64(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref int value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref uint value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref long value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref ulong value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref short value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref ushort value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref byte value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref sbyte value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref float value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref double value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref bool value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref char value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref string value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    protected override void Serialize(Span<byte> value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Fail(string message)
    {
        throw new FileFormatException(destination?.FullName ?? "<unknown>", message);
    }
}
