// <copyright file="BinarySerializationTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.IO;
using VoxelGame.Core.Serialization;
using Xunit;

namespace VoxelGame.Core.Tests.Serialization;

public class BinarySerializationTest
{
    [Fact]
    public void TestSerialization()
    {
        Data data = new()
        {
            o = "some string",
            q = Data.TestState.C
        };

        int[] p = [1, 2, 3, 4, 5];

        using MemoryStream stream = new();
        using BinarySerializer serializer = new(stream, "test");
        serializer.SerializeEntity(ref data);

        data.o = "another string";
        data.p = p;
        data.q = Data.TestState.A;
        data.r.Clear();

        stream.Position = 0;
        using BinaryDeserializer deserializer = new(stream, "test");
        deserializer.SerializeEntity(ref data);

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

        public readonly List<TestValue> r = [new TestValue {value = 21}, new TestValue {value = 22}];

        private int a = 1;
        private long b = 2;
        private int c = 3;
        private uint d = 4;
        private long e = 5;
        private ulong f = 6;
        private short g = 7;
        private ushort h = 8;
        private byte i = 9;
        private sbyte j = 10;
        private float k = 11.0f;
        private double l = 12.0;
        private bool m = true;
        private char n = '4';
        public string o = "fifteen";
        public int[] p = [16, 17, 18, 19, 20];
        public TestState q = TestState.B;
        public static int Version => 12;

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
            public int value;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref value);
            }
        }
    }
}
