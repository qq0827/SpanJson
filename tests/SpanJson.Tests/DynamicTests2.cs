using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using CuteAnt;
using SpanJson.Resolvers;
using SpanJson.Formatters.Dynamic;
using Xunit;

namespace SpanJson.Tests
{
    partial class DynamicTests
    {
        [Fact]
        public void DynamicObjectWithKnownMembersUtf16SnakeCase()
        {
            var list = new List<string> { "Hello", "World" };
            dynamic dynamicObject = new DynamicObjectWithKnownMembers2();
            dynamicObject.NumValue = 5;
            dynamicObject.GoodText = "Hello World";
            dynamicObject.JsonSupported = list;
            dynamicObject.DynamicValue = "Hello Universe";

            var serialized = JsonSerializer.Generic.Utf16.Serialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<char>>(dynamicObject);
            Assert.NotNull(serialized);
            Assert.Contains("\"good_text\":", serialized);
            Assert.Contains("\"num_value\":", serialized);
            Assert.Contains("\"json_supported\":", serialized);
            Assert.Contains("\"dynamic_value\":", serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<char>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(5, (int)deserialized.NumValue);
            var supported = (List<string>)deserialized.JsonSupported;
            Assert.NotEmpty(supported);
            Assert.Equal(list, supported);
            Assert.Equal("Hello World", (string)deserialized.good_text);
            Assert.Equal("Hello Universe", (string)deserialized.dynamic_value);
        }

        [Fact]
        public void DynamicObjectWithKnownMembersUtf8SnakeCase()
        {
            var list = new List<string> { "Hello", "World" };
            dynamic dynamicObject = new DynamicObjectWithKnownMembers2();
            dynamicObject.NumValue = 5;
            dynamicObject.GoodText = "Hello World";
            dynamicObject.JsonSupported = list;
            dynamicObject.DynamicValue = "Hello Universe";
            var serialized = JsonSerializer.Generic.Utf8.Serialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<byte>>(dynamicObject);
            Assert.NotNull(serialized);
            var serializedText = Encoding.UTF8.GetString(serialized);
            Assert.Contains("\"good_text\":", serializedText);
            Assert.Contains("\"num_value\":", serializedText);
            Assert.Contains("\"json_supported\":", serializedText);
            Assert.Contains("\"dynamic_value\":", serializedText);
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<byte>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(5, (int)deserialized.NumValue);
            var supported = (List<string>)deserialized.JsonSupported;
            Assert.NotEmpty(supported);
            Assert.Equal(list, supported);
            Assert.Equal("Hello World", (string)deserialized.good_text);
            Assert.Equal("Hello Universe", (string)deserialized.dynamic_value);
        }

        [Fact]
        public void JsonObjectTypeDeserializerTest()
        {
            var dict = new Dictionary<string, object>
            {
                { "KeyA", 101 },
                { "KeyB", Guid.NewGuid() },
                { "KeyC", CombGuid.NewComb() },
            };

            var json = JsonSerializer.Generic.Utf16.Serialize(dict);
            var newDict = JsonSerializer.Generic.Utf16.Deserialize<Dictionary<string, object>>(json);

            Assert.NotNull(newDict);
            Assert.Equal(3, newDict.Count);
            var utf16Num = newDict["KeyA"] as SpanJsonDynamicUtf16Number;
            Assert.NotNull(utf16Num);
            Assert.Equal(dict["KeyA"], (int)utf16Num);
            SpanJsonDynamicUtf16Number.DynamicConverter.TryConvertTo(typeof(int), utf16Num.Symbols, out object result);
            Assert.Equal(dict["KeyA"], result);

            var utf16Str = newDict["KeyB"] as SpanJsonDynamicUtf16String;
            Assert.NotNull(utf16Str);
            Assert.Equal(dict["KeyB"], (Guid)utf16Str);
            SpanJsonDynamicUtf16String.DynamicConverter.TryConvertTo(typeof(Guid), utf16Str.Symbols, out result);
            Assert.Equal(dict["KeyB"], result);

            utf16Str = newDict["KeyC"] as SpanJsonDynamicUtf16String;
            Assert.NotNull(utf16Str);
            Assert.Equal(dict["KeyC"], (CombGuid)utf16Str);
            SpanJsonDynamicUtf16String.DynamicConverter.TryConvertTo(typeof(CombGuid), utf16Str.Symbols, out result);
            Assert.Equal(dict["KeyC"], result);
        }

        [Fact]
        public void SerializeDeserializeDynamicChildUtf16_CombGuid()
        {
            var parent = new NonDynamicParent2();
            var child1 = new NonDynamicParent2.DynamicChild { Fixed = CombGuid.NewComb() };
            child1.Add("Id", CombGuid.NewComb());
            parent.Children.Add(child1);
            var child2 = new NonDynamicParent2.DynamicChild { Fixed = CombGuid.NewComb() };
            child2.Add("Name", "Hello World");
            parent.Children.Add(child2);
            var serialized = JsonSerializer.Generic.Utf16.Serialize(parent);
            Assert.NotNull(serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<NonDynamicParent2>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(parent.Children[0].Fixed, deserialized.Children[0].Fixed);
            Assert.Equal(parent.Children[1].Fixed, deserialized.Children[1].Fixed);
            dynamic dynamicChild1 = parent.Children[0];
            dynamic dynamicChild2 = parent.Children[1];
            dynamic deserializedDynamic = deserialized;
            Assert.Equal(dynamicChild1.Id, (CombGuid)deserializedDynamic.Children[0].Id);
            Assert.Equal(dynamicChild2.Name, (string)deserializedDynamic.Children[1].Name);
        }


        [Fact]
        public void SerializeDeserializeDynamicChildUtf8_CombGuid()
        {
            var parent = new NonDynamicParent2();
            var child1 = new NonDynamicParent2.DynamicChild { Fixed = CombGuid.NewComb() };
            child1.Add("Id", CombGuid.NewComb());
            parent.Children.Add(child1);
            var child2 = new NonDynamicParent2.DynamicChild { Fixed = CombGuid.NewComb() };
            child2.Add("Name", "Hello World");
            parent.Children.Add(child2);
            var serialized = JsonSerializer.Generic.Utf16.Serialize(parent);
            Assert.NotNull(serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<NonDynamicParent2>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(parent.Children[0].Fixed, deserialized.Children[0].Fixed);
            Assert.Equal(parent.Children[1].Fixed, deserialized.Children[1].Fixed);
            dynamic dynamicChild1 = parent.Children[0];
            dynamic dynamicChild2 = parent.Children[1];
            dynamic deserializedDynamic = deserialized;
            Assert.Equal(dynamicChild1.Id, (CombGuid)deserializedDynamic.Children[0].Id);
            Assert.Equal(dynamicChild2.Name, (string)deserializedDynamic.Children[1].Name);
        }

        public class DynamicObjectWithKnownMembers2 : DynamicObject
        {
            private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _dictionary.Keys;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (_dictionary.TryGetValue(binder.Name, out result))
                {
                    return true;
                }

                return base.TryGetMember(binder, out result);
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                _dictionary[binder.Name] = value;
                return true;
            }

            public int NumValue { get; set; }

            public int ReadOnly { get; } = 8;

            public IList<string> JsonSupported { get; set; }

            public AbstractMember NotSupported { get; set; }
        }

        public class NonDynamicParent2
        {
            public class DynamicChild : DynamicObject
            {
                public CombGuid Fixed { get; set; }
                public string Name { get; } = "Hello World";
                private static readonly string[] extraFields = new string[] { nameof(Fixed), nameof(Name) };
                private readonly Dictionary<string, object> _extra = new Dictionary<string, object>();

                public override IEnumerable<string> GetDynamicMemberNames()
                {
                    return _extra.Keys.Concat(extraFields);
                }

                public override bool TryGetMember(GetMemberBinder binder, out object result)
                {
                    return _extra.TryGetValue(binder.Name, out result);
                }

                public override bool TrySetMember(SetMemberBinder binder, object value)
                {
#if DESKTOPCLR
                    if (_extra.ContainsKey(binder.Name))
                    {
                        return false;
                    }
                    else
                    {
                        _extra[binder.Name] = value;
                        return true;
                    }
#else
                    return _extra.TryAdd(binder.Name, value);
#endif
                }

                public void Add(string key, object value)
                {
                    _extra.Add(key, value);
                }
            }

            public List<DynamicChild> Children { get; set; } = new List<DynamicChild>();
        }
    }
}