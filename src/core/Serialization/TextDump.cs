// <copyright file="TextDump.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Dumps a unit into a stream as text.
/// </summary>
public sealed class TextDump : Serializer, IDisposable
{
    private readonly TextWriter writer;

    /// <summary>
    ///     Creates a new text dump.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="signature">The signature of the unit to dump.</param>
    public TextDump(Stream stream, string signature)
    {
        writer = new StreamWriter(stream);

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
    public override void SerializeSmall(ref int value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void SerializeSmall(ref long value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref int value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref uint value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref long value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref ulong value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref short value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref ushort value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref byte value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref sbyte value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref float value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref double value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref bool value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref char value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    public override void Serialize(ref string value, string name = "")
    {
        writer.WriteLine($"{value} # {name}");
    }

    /// <inheritdoc />
    protected override void Serialize(Span<byte> value, string name = "")
    {
        writer.WriteLine($"{BitConverter.ToString(value.ToArray())} # {name}");
    }

    /// <inheritdoc />
    public override void Fail(string message)
    {
        throw new InvalidOperationException(message);
    }
}
