using System;
using System.Collections.Generic;
using SpanJson.Serialization;
using Xunit;

namespace SpanJson.Tests
{
    public class RuntimePolymorphicallyAttrTests : TestFixtureBase
    {
        static RuntimePolymorphicallyAttrTests()
        {
            typeof(Shape).AddRuntimeAttributes(new JsonPolymorphicallyAttribute());
        }

        [Fact]
        public void Utf16PolymorphicProperties()
        {
            var drawing = new Drawing
            {
                Shapes = new Shape[]
                {
                    new Square { Size = 10 },
                    new Square { Size = 20 },
                    new Circle { Radius = 5 }
                }
            };
            var utf16Json = JsonComplexSerializer.Instance.SerializeObject(drawing);
            var deserialized = JsonComplexSerializer.Instance.Deserialize<Drawing>(utf16Json);
            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);
        }

        [Fact]
        public void Utf8PolymorphicProperties()
        {
            var drawing = new Drawing
            {
                Shapes = new Shape[]
                {
                    new Square { Size = 10 },
                    new Square { Size = 20 },
                    new Circle { Radius = 5 }
                }
            };
            var utf8Json = JsonComplexSerializer.Instance.SerializeObjectToUtf8Bytes(drawing);
            var deserialized = JsonComplexSerializer.Instance.Deserialize<Drawing>(utf8Json);
            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);
        }

        [Fact]
        public void Utf16PolymorphicProperties1()
        {
            var drawing = new Drawing1
            {
                Shapes = new Shape[]
                {
                    new Square { Size = 10 },
                    new Square { Size = 20 },
                    new Circle { Radius = 5 }
                }
            };
            var utf16Json = JsonComplexSerializer.Instance.SerializeObject(drawing);
            var deserialized = JsonComplexSerializer.Instance.Deserialize<Drawing>(utf16Json);
            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);
        }

        [Fact]
        public void Utf8PolymorphicProperties1()
        {
            var drawing = new Drawing1
            {
                Shapes = new Shape[]
                {
                    new Square { Size = 10 },
                    new Square { Size = 20 },
                    new Circle { Radius = 5 }
                }
            };
            var utf8Json = JsonComplexSerializer.Instance.SerializeObjectToUtf8Bytes(drawing);
            var deserialized = JsonComplexSerializer.Instance.Deserialize<Drawing>(utf8Json);
            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);
        }

        public class Drawing
        {
            public Drawing()
            {
                Shapes = new List<Shape>();
            }

            public IList<Shape> Shapes { get; set; }
        }

        public class Drawing1
        {
            public Drawing1()
            {
                Shapes = new List<object>();
            }

            public IList<object> Shapes { get; set; }
        }

        public abstract class Shape
        {
            public int Id { get; set; }
        }

        public class Square : Shape
        {
            public int Size { get; set; }
            public string Name { get; set; }
        }

        public class Circle : Shape
        {
            public int Radius { get; set; }
            public byte[] BinName { get; set; }
        }
    }
}
