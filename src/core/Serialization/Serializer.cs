// <copyright file="Serializer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Memory;

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
    ///     Serialize an integer that is expected to be small but positive.
    ///     Serializers can use this to optimize for space usage.
    /// </summary>
    public abstract void SerializeSmall(ref Int32 value);

    /// <summary>
    ///     Serialize an unsigned integer that is expected to be small but positive.
    ///     Serializers can use this to optimize for space usage.
    /// </summary>
    public void SerializeSmall(ref UInt32 value)
    {
        var signed = (Int32) value;

        SerializeSmall(ref signed);

        value = (UInt32) signed;
    }

    /// <summary>
    ///     Serialize an unsigned integer that is expected to be small.
    ///     Serializers can use this to optimize for space usage.
    /// </summary>
    public abstract void SerializeSmall(ref Int64 value);

    /// <summary>
    ///     Serialize an unsigned integer that is expected to be small.
    ///     Serializers can use this to optimize for space usage.
    /// </summary>
    public void SerializeSmall(ref UInt64 value)
    {
        var signed = (Int64) value;

        SerializeSmall(ref signed);

        value = (UInt64) signed;
    }

    /// <summary>
    ///     Serialize an integer.
    /// </summary>
    public abstract void Serialize(ref Int32 value);

    /// <summary>
    ///     Serialize an unsigned integer.
    /// </summary>
    public abstract void Serialize(ref UInt32 value);

    /// <summary>
    ///     Serialize a long.
    /// </summary>
    public abstract void Serialize(ref Int64 value);

    /// <summary>
    ///     Serialize an unsigned long.
    /// </summary>
    public abstract void Serialize(ref UInt64 value);

    /// <summary>
    ///     Serialize a short.
    /// </summary>
    public abstract void Serialize(ref Int16 value);

    /// <summary>
    ///     Serialize an unsigned short.
    /// </summary>
    public abstract void Serialize(ref UInt16 value);

    /// <summary>
    ///     Serialize a byte.
    /// </summary>
    public abstract void Serialize(ref Byte value);

    /// <summary>
    ///     Serialize an unsigned byte.
    /// </summary>
    public abstract void Serialize(ref SByte value);

    /// <summary>
    ///     Serialize a float.
    /// </summary>
    public abstract void Serialize(ref Single value);

    /// <summary>
    ///     Serialize a double.
    /// </summary>
    public abstract void Serialize(ref Double value);

    /// <summary>
    ///     Serialize a bool.
    /// </summary>
    public abstract void Serialize(ref Boolean value);

    /// <summary>
    ///     Serialize a char.
    /// </summary>
    public abstract void Serialize(ref Char value);

    /// <summary>
    ///     Serialize a string. Also serializes the length of the string.
    /// </summary>
    public abstract void Serialize(ref String value);

    /// <summary>
    ///     Serialize an array of values unmanaged values and its length.
    ///     This is not equivalent to serializing each value individually, as the method is allowed to optimize the
    ///     serialization.
    ///     If the passed array has correct length, it will be used, otherwise a new array will be created.
    /// </summary>
    public void Serialize<T>(ref T[] value)
        where T : unmanaged
    {
        Int32 length = value.Length;
        SerializeSmall(ref length);

        if (value.Length != length) value = new T[length];

        Span<Byte> span = MemoryMarshal.AsBytes(value.AsSpan());
        Serialize(span);
    }

    /// <summary>
    ///     Serialize a segment.
    ///     Will not serialize the number of entries in the segment.
    /// </summary>
    /// <param name="segment">The segment to serialize.</param>
    /// <typeparam name="T">The type of the values in the segment.</typeparam>
    public void Serialize<T>(NativeSegment<T> segment) where T : unmanaged
    {
        Span<T> content = segment.AsSpan();
        Span<Byte> bytes = MemoryMarshal.AsBytes(content);

        Serialize(bytes);
    }

    /// <summary>
    ///     Serialize a span of bytes. Does NOT serialize the length of the span.
    /// </summary>
    protected abstract void Serialize(Span<Byte> value);

    /// <summary>
    ///     Serialize an enum.
    /// </summary>
    public void Serialize<T>(ref T value)
        where T : unmanaged, Enum
    {
        Int64 data = default;

        if (Unsafe.SizeOf<T>() == sizeof(Int32)) data = Unsafe.As<T, Int32>(ref value);
        else if (Unsafe.SizeOf<T>() == sizeof(Byte)) data = Unsafe.As<T, Byte>(ref value);
        else if (Unsafe.SizeOf<T>() == sizeof(Int16)) data = Unsafe.As<T, Int16>(ref value);
        else if (Unsafe.SizeOf<T>() == sizeof(Int64)) data = Unsafe.As<T, Int64>(ref value);
        else Fail($"Unsupported enum size: {Unsafe.SizeOf<T>()}");

        SerializeSmall(ref data);

        if (Unsafe.SizeOf<T>() == sizeof(Int32))
        {
            var small = (Int32) data;
            value = Unsafe.As<Int32, T>(ref small);
        }
        else if (Unsafe.SizeOf<T>() == sizeof(Byte))
        {
            var small = (Byte) data;
            value = Unsafe.As<Byte, T>(ref small);
        }
        else if (Unsafe.SizeOf<T>() == sizeof(Int16))
        {
            var small = (Int16) data;
            value = Unsafe.As<Int16, T>(ref small);
        }
        else if (Unsafe.SizeOf<T>() == sizeof(Int64))
        {
            value = Unsafe.As<Int64, T>(ref data);
        }
    }

    /// <summary>
    ///     Serialize a value.
    /// </summary>
    public void SerializeValue<T>(ref T value)
        where T : IValue
    {
        value.Serialize(this);
    }

    /// <summary>
    ///     Serialize a list of values. This is equivalent to serializing each value individually.
    ///     The passed list will be modified, e.g. resized and some entries might be cleared.
    /// </summary>
    public void SerializeValues<T>(IList<T> values)
        where T : IValue, new()
    {
        Int32 count = values.Count;
        SerializeSmall(ref count);

        for (var index = 0; index < count; index++)
        {
            if (index >= values.Count) values.Add(new T());

            T value = values[index];
            SerializeValue(ref value);
            values[index] = value;
        }

        for (Int32 index = values.Count - 1; index >= count; index--) values.RemoveAt(index);
    }

    /// <summary>
    ///     Serialize a custom array.
    ///     The length of the array is not serialized, so it must be constant.
    /// </summary>
    /// <param name="values">The array to serialize.</param>
    /// <typeparam name="T">The type of the array.</typeparam>
    public void SerializeValues<T>(IArray<T> values)
        where T : IValue
    {
        for (var index = 0; index < values.Count; index++)
        {
            T value = values[index];
            SerializeValue(ref value);
            values[index] = value;
        }
    }

    /// <summary>
    ///     Serialize an entity.
    /// </summary>
    public void SerializeEntity<T>(T entity)
        where T : IEntity
    {
        UInt32 version = T.Version;

        Serialize(ref version);

        if (version > T.Version)
            Fail($"Entity {typeof(T).Name} has been serialized with a newer version {version} than the current {T.Version}.");

        entity.Serialize(this, new IEntity.Header(version));
    }

    /// <summary>
    ///     Serialize a list of entities. This is equivalent to serializing each entity individually.
    ///     The list size must exactly match the number of entities to be serialized.
    /// </summary>
    public void SerializeEntities<T>(IList<T> entities)
        where T : IEntity
    {
        Int32 count = entities.Count;
        SerializeSmall(ref count);

        if (entities.Count != count)
            Fail($"Expected {count} entities, but got {entities.Count}.");

        for (var index = 0; index < count; index++)
        {
            T entity = entities[index];
            SerializeEntity(entity);
            entities[index] = entity;
        }
    }

    /// <summary>
    ///     Ensure that a signature is present.
    /// </summary>
    public void Signature(String content)
    {
        for (var index = 0; index < content.Length; index++)
        {
            Char expected = content[index];
            Char actual = expected;

            Serialize(ref actual);

            if (actual != expected) Fail($"Expected signature {content}, but got {actual} at position {index}.");
        }
    }

    /// <summary>
    ///     Fail the serialization process.
    /// </summary>
    /// <param name="message">A message describing the failure.</param>
    public abstract void Fail(String message);
}
