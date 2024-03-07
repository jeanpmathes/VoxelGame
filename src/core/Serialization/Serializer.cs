// <copyright file="Serializer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Base class for serializers, which offer both serialization and deserialization trough a common interface.
/// </summary>
public abstract class Serializer
{
    /// <summary>
    ///     Get information about the unit that is currently being serialized.
    /// </summary>
    protected abstract UnitHeader Unit { get; }

    /// <summary>
    ///     Serialize an integer that is expected to be small.
    ///     Serializers can use this to optimize for space usage.
    /// </summary>
    public abstract void SerializeSmall(ref int value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an unsigned integer that is expected to be small.
    ///     Serializers can use this to optimize for space usage.
    /// </summary>
    public abstract void SerializeSmall(ref long value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an integer.
    /// </summary>
    public abstract void Serialize(ref int value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an unsigned integer.
    /// </summary>
    public abstract void Serialize(ref uint value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a long.
    /// </summary>
    public abstract void Serialize(ref long value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an unsigned long.
    /// </summary>
    public abstract void Serialize(ref ulong value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a short.
    /// </summary>
    public abstract void Serialize(ref short value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an unsigned short.
    /// </summary>
    public abstract void Serialize(ref ushort value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a byte.
    /// </summary>
    public abstract void Serialize(ref byte value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an unsigned byte.
    /// </summary>
    public abstract void Serialize(ref sbyte value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a float.
    /// </summary>
    public abstract void Serialize(ref float value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a double.
    /// </summary>
    public abstract void Serialize(ref double value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a bool.
    /// </summary>
    public abstract void Serialize(ref bool value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a char.
    /// </summary>
    public abstract void Serialize(ref char value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize a string. Also serializes the length of the string.
    /// </summary>
    public abstract void Serialize(ref string value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an array of values unmanaged values and its length.
    ///     This is not equivalent to serializing each value individually, as the method is allowed to optimize the
    ///     serialization.
    ///     If the passed array has correct length, it will be used, otherwise a new array will be created.
    /// </summary>
    public void Serialize<T>(ref T[] value, [CallerArgumentExpression(nameof(value))] string name = "")
        where T : unmanaged
    {
        int length = value.Length;
        Serialize(ref length, name);

        if (value.Length != length) value = new T[length];

        Span<byte> span = MemoryMarshal.AsBytes(value.AsSpan());
        Serialize(span, name);
    }

    /// <summary>
    ///     Serialize a span of bytes. Does NOT serialize the length of the span.
    /// </summary>
    protected abstract void Serialize(Span<byte> value, [CallerArgumentExpression(nameof(value))] string name = "");

    /// <summary>
    ///     Serialize an enum.
    /// </summary>
    public void Serialize<T>(ref T value, [CallerArgumentExpression(nameof(value))] string name = "")
        where T : unmanaged, Enum
    {
        long data = default;

        if (Unsafe.SizeOf<T>() == sizeof(int)) data = Unsafe.As<T, int>(ref value);
        else if (Unsafe.SizeOf<T>() == sizeof(byte)) data = Unsafe.As<T, byte>(ref value);
        else if (Unsafe.SizeOf<T>() == sizeof(short)) data = Unsafe.As<T, short>(ref value);
        else if (Unsafe.SizeOf<T>() == sizeof(long)) data = Unsafe.As<T, long>(ref value);
        else Fail($"Unsupported enum size: {Unsafe.SizeOf<T>()}");

        SerializeSmall(ref data, name);

        if (Unsafe.SizeOf<T>() == sizeof(int))
        {
            var small = (int) data;
            value = Unsafe.As<int, T>(ref small);
        }
        else if (Unsafe.SizeOf<T>() == sizeof(byte))
        {
            var small = (byte) data;
            value = Unsafe.As<byte, T>(ref small);
        }
        else if (Unsafe.SizeOf<T>() == sizeof(short))
        {
            var small = (short) data;
            value = Unsafe.As<short, T>(ref small);
        }
        else if (Unsafe.SizeOf<T>() == sizeof(long))
        {
            value = Unsafe.As<long, T>(ref data);
        }
    }

    /// <summary>
    ///     Serialize a value.
    /// </summary>
    public void SerializeValue<T>(ref T value, [CallerArgumentExpression(nameof(value))] string name = "")
        where T : IValue
    {
        value.Serialize(this);
    }

    /// <summary>
    ///     Serialize a list of values. This is equivalent to serializing each value individually.
    ///     The passed list will be modified, e.g. resized and some entries might be cleared.
    /// </summary>
    public void SerializeValues<T>(IList<T> values, [CallerArgumentExpression(nameof(values))] string name = "")
        where T : IValue, new()
    {
        int count = values.Count;
        Serialize(ref count, name);

        for (var index = 0; index < count; index++)
        {
            if (index >= values.Count) values.Add(new T());

            T value = values[index];
            SerializeValue(ref value);
            values[index] = value;
        }

        for (int index = values.Count - 1; index >= count; index--) values.RemoveAt(index);
    }

    /// <summary>
    ///     Serialize an entity.
    /// </summary>
    public void SerializeEntity<T>(ref T entity, [CallerArgumentExpression(nameof(entity))] string name = "")
        where T : IEntity
    {
        int version = T.Version;

        if (Unit.Version <= MetaVersion.Initial) Serialize(ref version, name);
#pragma warning disable S3717
        else throw new NotImplementedException("Entity headers are not implemented for the current version of the serialization system.");
#pragma warning restore S3717

        entity.Serialize(this, new IEntity.Header(version));
    }

    /// <summary>
    ///     Serialize a list of entities. This is equivalent to serializing each entity individually.
    /// </summary>
    public void SerializeEntities<T>(IList<T> entities, [CallerArgumentExpression(nameof(entities))] string name = "")
        where T : IEntity, new()
    {
        int count = entities.Count;
        Serialize(ref count, name);

        for (var index = 0; index < count; index++)
        {
            if (index >= entities.Count) entities.Add(new T());

            T entity = entities[index];
            SerializeEntity(ref entity);
            entities[index] = entity;
        }

        for (int index = entities.Count - 1; index >= count; index--) entities.RemoveAt(index);
    }

    /// <summary>
    ///     Ensure that a signature is present.
    /// </summary>
    public void Signature(string content)
    {
        for (var index = 0; index < content.Length; index++)
        {
            char expected = content[index];
            char actual = expected;

            Serialize(ref actual);

            if (actual != expected) Fail($"Expected signature {content}, but got {actual} at position {index}");
        }
    }

    /// <summary>
    ///     Fail the serialization process.
    /// </summary>
    /// <param name="message">A message describing the failure.</param>
    public abstract void Fail(string message);
}
