// <copyright file="BinarySerializationTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using VoxelGame.Core.Serialization;
using Xunit;

namespace VoxelGame.Core.Tests.Serialization;

[TestSubject(typeof(BinaryDeserializer))]
[TestSubject(typeof(BinaryDeserializer))]
public class BinarySerializationTests
{
    [Fact]
    public void IEntity_ShouldHaveSameStateBeforeAndAfterSerialization()
    {
        Data data = new()
        {
            o = "some string",
            q = Data.TestState.C
        };

        Int32[] p = [1, 2, 3, 4, 5];

        using MemoryStream stream = new();
        using BinarySerializer serializer = new(stream, "test");
        serializer.SerializeEntity(data);

        data.o = "another string";
        data.p = p;
        data.q = Data.TestState.A;
        data.r.Clear();

        stream.Position = 0;
        using BinaryDeserializer deserializer = new(stream, "test");
        deserializer.SerializeEntity(data);

        Assert.Equal("some string", data.o);
        Assert.Same(p, data.p);
        Assert.Equal(Data.TestState.C, data.q);
        Assert.NotEmpty(data.r);
    }

    private class Data : IEntity
    {
        public enum TestState
        {
            A = 100,
            B = 200,
            C = 300
        }

        public readonly List<TestValue> r =
        [
            new()
                {value = 21},
            new()
                {value = 22}
        ];

        private Int32 a = 1;
        private Int64 b = 2;
        private Int32 c = 3;
        private UInt32 d = 4;
        private Int64 e = 5;
        private UInt64 f = 6;
        private Int16 g = 7;
        private UInt16 h = 8;
        private Byte i = 9;
        private SByte j = 10;
        private Single k = 11.0f;
        private Double l = 12.0;
        private Boolean m = true;
        private Char n = '4';
        public String o = "fifteen";
        public Int32[] p = [16, 17, 18, 19, 20];
        public TestState q = TestState.B;

        public static UInt32 Version => 12;

        public void Serialize(Serializer serializer, IEntity.Header header)
        {
            Assert.Equal(Version, header.Version);

            serializer.Serialize(ref a);
            serializer.Serialize(ref b);
            serializer.Serialize(ref c);
            serializer.Serialize(ref d);
            serializer.Serialize(ref e);
            serializer.Serialize(ref f);
            serializer.Serialize(ref g);
            serializer.Serialize(ref h);
            serializer.Serialize(ref i);
            serializer.Serialize(ref j);
            serializer.Serialize(ref k);
            serializer.Serialize(ref l);
            serializer.Serialize(ref m);
            serializer.Serialize(ref n);
            serializer.Serialize(ref o);
            serializer.Serialize(ref p);
            serializer.Serialize(ref q);
            serializer.SerializeValues(r);
        }

        public class TestValue : IValue
        {
            public Int32 value;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref value);
            }
        }
    }
}
