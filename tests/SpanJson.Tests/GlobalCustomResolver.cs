using System;
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
            var jsonStr = JsonSerializer.Generic.Utf8.Serialize<Dog, CustomResolver<byte>>(dog);
            var newdog = JsonSerializer.Generic.Utf8.Deserialize<Dog, CustomResolver<byte>>(jsonStr);
            Assert.Equal(dog.Name + "_y", newdog.Name);

            var cat = new Cat("cat_456");
            jsonStr = JsonSerializer.Generic.Utf8.Serialize<Cat, CustomResolver<byte>>(cat);
            var newcat = JsonSerializer.Generic.Utf8.Deserialize<Cat, CustomResolver<byte>>(jsonStr);
            Assert.Equal(cat.Name + "_y", newcat.Name);
        }

        [Fact]
        public void SerializeDeserializeUtf16()
        {
            var dog = new Dog("dog_123");
            var jsonStr = JsonSerializer.Generic.Utf16.Serialize<Dog, CustomResolver<char>>(dog);
            var newdog = JsonSerializer.Generic.Utf16.Deserialize<Dog, CustomResolver<char>>(jsonStr);
            Assert.Equal(dog.Name + "_x", newdog.Name);

            var cat = new Cat("cat_456");
            jsonStr = JsonSerializer.Generic.Utf16.Serialize<Cat, CustomResolver<char>>(cat);
            var newcat = JsonSerializer.Generic.Utf16.Deserialize<Cat, CustomResolver<char>>(jsonStr);
            Assert.Equal(cat.Name + "_x", newcat.Name);
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
                if (typeof(Animal).IsAssignableFrom(type))
                {
                    return Activator.CreateInstance(typeof(AnimalFormatter<>).MakeGenericType(type)) as ICustomJsonFormatter;
                }
                return null;
            }
        }

        public class AnimalFormatter<T> : ICustomJsonFormatter<T>
            where T : Animal
        {
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

        public abstract class Animal
        {
            public abstract string Name { get; set; }
        }

        public class Dog : Animal
        {
            public Dog(string name) => Name = name;
            public override string Name { get; set; }
        }

        public class Cat : Animal
        {
            public Cat(string name) => Name = name;
            public override string Name { get; set; }
        }
    }
}
