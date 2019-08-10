using System;
using System.Collections.Generic;
using SpanJson.Document;
using SpanJson.Linq;
using SpanJson.Internal;
using SpanJson.Serialization;
using Xunit;

namespace SpanJson.Tests
{
    public class PolymorphicallyTests : TestFixtureBase
    {
        [Fact]
        public void GetElementType()
        {
            var type = typeof(Dictionary<int, string>);
            var elementType = JsonClassInfo.GetElementType(type);
            Assert.Equal(typeof(string), elementType);
        }

        [Fact]
        public void Utf16PolymorphicProperties_Err()
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
            var utf16Json = JsonSerializer.Generic.Utf16.Serialize(drawing);
            Assert.Throws<JsonParserException>(() => JsonSerializer.Generic.Utf16.Deserialize<Drawing>(utf16Json));
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
        public void Utf8PolymorphicProperties_Err()
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
            var utf8Json = JsonSerializer.Generic.Utf8.Serialize(drawing);
            Assert.Throws<JsonParserException>(() => JsonSerializer.Generic.Utf8.Deserialize<Drawing>(utf8Json));
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
        public void PolymorphicLinq_Err()
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
            var jObj = JObject.FromObject(drawing);
            Assert.Throws<JsonParserException>(() => jObj.ToObject<Drawing>());
        }

        [Fact]
        public void PolymorphicLinq()
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
            var jObj = JObject.FromPolymorphicObject(drawing);
            var deserialized = jObj.ToPolymorphicObject<Drawing>();
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
        public void Utf16PolymorphicLinq()
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

            var jsPool = JToken.PolymorphicSerializerPool;
            var js = jsPool.Take();
            var utf16Json = Newtonsoft.Json.Linq.JObject.FromObject(drawing, js).ToString();
            jsPool.Return(js);

            var jObj = JObject.Parse(utf16Json);
            var deserialized = jObj.ToPolymorphicObject<Drawing>();

            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);

            jObj = JObject.FromObject(JsonSerializer.Generic.Utf16.Deserialize<dynamic>(utf16Json.ToCharArray()));
            deserialized = jObj.ToPolymorphicObject<Drawing>();

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
        public void Utf8PolymorphicLinq()
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

            var jsPool = JToken.PolymorphicSerializerPool;
            var js = jsPool.Take();
            var utf16Json = Newtonsoft.Json.Linq.JObject.FromObject(drawing, js).ToString();
            var utf8Json = TextEncodings.UTF8NoBOM.GetBytes(utf16Json);
            jsPool.Return(js);

            var jObj = JObject.Parse(utf8Json);
            var deserialized = jObj.ToPolymorphicObject<Drawing>();

            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);

            jObj = JObject.FromObject(JsonSerializer.Generic.Utf8.Deserialize<dynamic>(utf8Json));
            deserialized = jObj.ToPolymorphicObject<Drawing>();

            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);

            jObj = JObject.FromObject(JsonDocument.Parse(utf8Json, useArrayPools: false));
            deserialized = jObj.ToPolymorphicObject<Drawing>();

            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Shapes.Count);
            Assert.Equal(typeof(Square), deserialized.Shapes[0].GetType());
            Assert.Equal(10, ((Square)deserialized.Shapes[0]).Size);
            Assert.Equal(typeof(Square), deserialized.Shapes[1].GetType());
            Assert.Equal(20, ((Square)deserialized.Shapes[1]).Size);
            Assert.Equal(typeof(Circle), deserialized.Shapes[2].GetType());
            Assert.Equal(5, ((Circle)deserialized.Shapes[2]).Radius);

            jObj = JObject.FromObject(JsonDocument.Parse(utf8Json, useArrayPools: false).RootElement);
            deserialized = jObj.ToPolymorphicObject<Drawing>();

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

            [JsonPolymorphically]
            public IList<Shape> Shapes { get; set; }
        }

        public abstract class Shape
        {
            public int Id { get; set; }
        }

        public class Square : Shape
        {
            public int Size { get; set; }
        }

        public class Circle : Shape
        {
            public int Radius { get; set; }
        }
    }
}
