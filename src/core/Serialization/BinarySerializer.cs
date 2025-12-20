// <copyright file="BinarySerializer.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
    private readonly FileInfo? destination;
    private readonly BinaryWriter writer;

    /// <summary>
    ///     Create a new binary serializer.
    ///     Will begin a new unit.
    /// </summary>
    /// <param name="stream">The stream to write to. Will close the stream when disposed.</param>
    /// <param name="signature">The signature of the specific format.</param>
    /// <param name="file">The file that is being written to, if any.</param>
    public BinarySerializer(Stream stream, String signature, FileInfo? file = null)
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
    public override void SerializeSmall(ref Int32 value)
    {
        writer.Write7BitEncodedInt(value);
    }

    /// <inheritdoc />
    public override void SerializeSmall(ref Int64 value)
    {
        writer.Write7BitEncodedInt64(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Int32 value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref UInt32 value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Int64 value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref UInt64 value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Int16 value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref UInt16 value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Byte value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref SByte value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Single value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Double value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Boolean value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref Char value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref String value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    protected override void Serialize(Span<Byte> value)
    {
        writer.Write(value);
    }

    /// <inheritdoc />
    public override void Fail(String message)
    {
        throw new FileFormatException(destination?.FullName ?? "<unknown>", message);
    }
}
