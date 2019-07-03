using System;
using System.Collections.Generic;
using System.Dynamic;
using SpanJson.Resolvers;
using Xunit;

namespace SpanJson.Tests
{
    public class GlobalCustomResolverTests
    {
        public GlobalCustomResolverTests()
        {
            CustomResolver<char>.TryRegisterGlobalCustomrResolver(AnimalResolver.Instance);
            CustomResolver<byte>.TryRegisterGlobalCustomrResolver(AnimalResolver.Instance);
        }

        [Fact]
        public void SerializeDeserializeFailure()
        {
            var dog = new Dog("dog");
            Assert.Throws<TypeInitializationException>(() => JsonSerializer.Generic.Utf16.Serialize(dog));
            Assert.Throws<TypeInitializationException>(() => JsonSerializer.Generic.Utf8.Serialize(dog));
        }

        [Fact]
        public void SerializeDeserializeUtf8()
        {
            var dog = new Dog("dog_123");
            var json = JsonSerializer.Generic.Utf8.Serialize<Dog, CustomResolver<byte>>(dog);
            var newdog = JsonSerializer.Generic.Utf8.Deserialize<Dog, CustomResolver<byte>>(json);
            Assert.Equal(dog.Name + "_y", newdog.Name);

            var cat = new Cat("cat_456");
            json = JsonSerializer.Generic.Utf8.Serialize<Cat, CustomResolver<byte>>(cat);
            var newcat = JsonSerializer.Generic.Utf8.Deserialize<Cat, CustomResolver<byte>>(json);
            Assert.Equal(cat.Name + "_y", newcat.Name);
        }

        [Fact]
        public void SerializeDeserializeUtf16()
        {
            var dog = new Dog("dog_123");
            var json = JsonSerializer.Generic.Utf16.Serialize<Dog, CustomResolver<char>>(dog);
            var newdog = JsonSerializer.Generic.Utf16.Deserialize<Dog, CustomResolver<char>>(json);
            Assert.Equal(dog.Name + "_x", newdog.Name);

            var cat = new Cat("cat_456");
            json = JsonSerializer.Generic.Utf16.Serialize<Cat, CustomResolver<char>>(cat);
            var newcat = JsonSerializer.Generic.Utf16.Deserialize<Cat, CustomResolver<char>>(json);
            Assert.Equal(cat.Name + "_x", newcat.Name);
        }

        [Fact]
        public void SerializeDeserializeAsPropertyUtf8()
        {
            var expectedAnimal = new AnimalWrapper { Age = 101, Animal = new Dog("dog_123") };
            var json = JsonSerializer.Generic.Utf8.Serialize<AnimalWrapper, CustomResolver<byte>>(expectedAnimal);
            var animal = JsonSerializer.Generic.Utf8.Deserialize<AnimalWrapper, CustomResolver<byte>>(json);
            Assert.Equal(expectedAnimal.Age, animal.Age);
            Assert.Equal(expectedAnimal.Animal.Name + "_y", animal.Animal.Name);

            expectedAnimal = new AnimalWrapper { Age = 102, Animal = new Cat("cat_456") };
            json = JsonSerializer.Generic.Utf8.Serialize<AnimalWrapper, CustomResolver<byte>>(expectedAnimal);
            animal = JsonSerializer.Generic.Utf8.Deserialize<AnimalWrapper, CustomResolver<byte>>(json);
            Assert.Equal(expectedAnimal.Age, animal.Age);
            Assert.Equal(expectedAnimal.Animal.Name + "_y", animal.Animal.Name);
        }

        [Fact]
        public void SerializeDeserializeAsPropertyUtf16()
        {
            var expectedAnimal = new AnimalWrapper { Age = 101, Animal = new Dog("dog_123") };
            var json = JsonSerializer.Generic.Utf16.Serialize<AnimalWrapper, CustomResolver<char>>(expectedAnimal);
            var animal = JsonSerializer.Generic.Utf16.Deserialize<AnimalWrapper, CustomResolver<char>>(json);
            Assert.Equal(expectedAnimal.Age, animal.Age);
            Assert.Equal(expectedAnimal.Animal.Name + "_x", animal.Animal.Name);

            expectedAnimal = new AnimalWrapper { Age = 102, Animal = new Cat("cat_456") };
            json = JsonSerializer.Generic.Utf16.Serialize<AnimalWrapper, CustomResolver<char>>(expectedAnimal);
            animal = JsonSerializer.Generic.Utf16.Deserialize<AnimalWrapper, CustomResolver<char>>(json);
            Assert.Equal(expectedAnimal.Age, animal.Age);
            Assert.Equal(expectedAnimal.Animal.Name + "_x", animal.Animal.Name);
        }

        [Fact]
        public void DynamicObjectWithKnownMembersUtf16()
        {
            dynamic dynamicObject = new DynamicObjectWithKnownMembers();
            dynamicObject.Value = 5;
            dynamicObject.Text = "Hello World";
            dynamicObject.SupportedAnimal = new Dog("dog_123");

            var serialized = JsonSerializer.Generic.Utf16.Serialize<DynamicObjectWithKnownMembers, CustomResolver<char>>(dynamicObject);
            Assert.NotNull(serialized);
            var obj = JsonSerializer.Generic.Utf16.Deserialize<DynamicObjectWithKnownMembers, CustomResolver<char>>(serialized);
            Assert.Equal(dynamicObject.SupportedAnimal.Name + "_x", obj.SupportedAnimal.Name);
        }

        [Fact]
        public void DynamicObjectWithKnownMembersUtf8()
        {
            dynamic dynamicObject = new DynamicObjectWithKnownMembers();
            dynamicObject.Value = 5;
            dynamicObject.Text = "Hello World";
            dynamicObject.SupportedAnimal = new Cat("cat_456");

            var serialized = JsonSerializer.Generic.Utf8.Serialize<DynamicObjectWithKnownMembers, CustomResolver<byte>>(dynamicObject);
            Assert.NotNull(serialized);
            var obj = JsonSerializer.Generic.Utf8.Deserialize<DynamicObjectWithKnownMembers, CustomResolver<byte>>(serialized);
            Assert.Equal(dynamicObject.SupportedAnimal.Name + "_y", obj.SupportedAnimal.Name);
        }

        public sealed class CustomResolver<TSymbol> : ResolverBase<TSymbol, CustomResolver<TSymbol>> where TSymbol : struct
        {
            public CustomResolver() : base(new SpanJsonOptions())
            {
                //RegisterGlobalCustomrResolver(new AnimalResolver());
            }
        }

        public class AnimalResolver : ICustomJsonFormatterResolver
        {
            public static readonly AnimalResolver Instance = new AnimalResolver();

            public ICustomJsonFormatter GetFormatter(Type type)
            {
                if (typeof(IAnimal).IsAssignableFrom(type))
                {
                    return Activator.CreateInstance(typeof(AnimalFormatter<>).MakeGenericType(type)) as ICustomJsonFormatter;
                }
                return null;
            }

            public bool IsSupportedType(Type type)
            {
                return typeof(IAnimal).IsAssignableFrom(type);
            }
        }

        public class AnimalFormatter<T> : ICustomJsonFormatter<T>
            where T : class, IAnimal
        {
            public static readonly AnimalFormatter<T> Default = new AnimalFormatter<T>();

            public object Arguments { get; set; }

            public T Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
            {
                var name = reader.ReadUtf8String();
                if (name.StartsWith("dog", StringComparison.OrdinalIgnoreCase))
                {
                    return new Dog(name + "_y") as T;
                }
                else if (name.StartsWith("cat", StringComparison.OrdinalIgnoreCase))
                {
                    return new Cat(name + "_y") as T;
                }
                throw new NotImplementedException();
            }

            public T Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
            {
                var name = reader.ReadUtf16String();
                if (name.StartsWith("dog", StringComparison.OrdinalIgnoreCase))
                {
                    return new Dog(name + "_x") as T;
                }
                else if (name.StartsWith("cat", StringComparison.OrdinalIgnoreCase))
                {
                    return new Cat(name + "_x") as T;
                }
                throw new NotImplementedException();
            }

            public void Serialize(ref JsonWriter<byte> writer, T value, IJsonFormatterResolver<byte> resolver)
            {
                writer.WriteUtf8String(value.Name);
            }

            public void Serialize(ref JsonWriter<char> writer, T value, IJsonFormatterResolver<char> resolver)
            {
                writer.WriteUtf16String(value.Name);
            }
        }

        public class AnimalWrapperI
        {
            public int Age { get; set; }

            public IAnimal Animal { get; set; }
        }

        public class AnimalWrapper
        {
            public int Age { get; set; }

            public Animal Animal { get; set; }
        }

        public interface IAnimal
        {
            string Name { get; set; }
        }

        public abstract class Animal : IAnimal
        {
            public abstract string Name { get; set; }
        }

        public class Dog : Animal
        {
            internal Dog(string name) => Name = name;
            public override string Name { get; set; }
        }

        public class Cat : Animal
        {
            internal Cat(string name) => Name = name;
            public override string Name { get; set; }
        }

        public class DynamicObjectWithKnownMembers : DynamicObject
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

            public int Value { get; set; }

            public int ReadOnly { get; } = 8;

            public IList<string> Supported { get; set; }

            public IAnimal SupportedAnimal { get; set; }
        }
    }
}
