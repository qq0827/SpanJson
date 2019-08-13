// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using SpanJson.Resolvers;
using SpanJson.Serialization;
using Xunit;

namespace SpanJson.Tests
{
    public static class CyclicTests
    {
        [Fact]
        public static void WriteCyclicFailDefault()
        {
            TestClassWithCycle obj = new TestClassWithCycle();
            obj.Parent = obj;

            // We don't allow graph cycles; we throw JsonException instead of an unrecoverable StackOverflow.
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Generic.Utf16.Serialize(obj));
        }

        private static TestClassWithCycle CreateObjectHierarchy(int i, int max, TestClassWithCycle previous)
        {
            if (i == max)
            {
                return null;
            }

            var obj = new TestClassWithCycle(i.ToString());
            previous.Parent = obj;
            return CreateObjectHierarchy(++i, max, obj);
        }

        [Fact]
        public static void SimpleTypeCycle()
        {
            TestClassWithArrayOfElementsOfTheSameClass obj = new TestClassWithArrayOfElementsOfTheSameClass();

            // A cycle in just Types (not data) is allowed.
            string json = JsonSerializer.Generic.Utf16.Serialize<TestClassWithArrayOfElementsOfTheSameClass, IncludeNullsOriginalCaseResolver<char>>(obj);
            Assert.Equal(@"{""Array"":null}", json);
        }

        [Fact]
        public static void DeepTypeCycleWithRoundTrip()
        {
            TestClassWithCycle root = new TestClassWithCycle("root");
            TestClassWithCycle parent = new TestClassWithCycle("parent");
            root.Parent = parent;
            root.Children.Add(new TestClassWithCycle("child1"));
            root.Children.Add(new TestClassWithCycle("child2"));

            // A cycle in just Types (not data) is allowed.
            string json = JsonSerializer.Generic.Utf16.Serialize(root);

            // Round-trip the JSON.
            TestClassWithCycle rootCopy = JsonSerializer.Generic.Utf16.Deserialize<TestClassWithCycle>(json);
            Assert.Equal("root", rootCopy.Name);
            Assert.Equal(2, rootCopy.Children.Count);

            Assert.Equal("parent", rootCopy.Parent.Name);
            Assert.Empty(rootCopy.Parent.Children);
            Assert.Null(rootCopy.Parent.Parent);

            Assert.Equal("child1", rootCopy.Children[0].Name);
            Assert.Empty(rootCopy.Children[0].Children);
            Assert.Null(rootCopy.Children[0].Parent);

            Assert.Equal("child2", rootCopy.Children[1].Name);
            Assert.Empty(rootCopy.Children[1].Children);
            Assert.Null(rootCopy.Children[1].Parent);
        }

        public class TestClassWithCycle
        {
            public TestClassWithCycle() { }

            public TestClassWithCycle(string name)
            {
                Name = name;
            }

            public TestClassWithCycle Parent { get; set; }
            public List<TestClassWithCycle> Children { get; set; } = new List<TestClassWithCycle>();
            public string Name { get; set; }
        }

        public class TestClassWithArrayOfElementsOfTheSameClass
        {
            public TestClassWithArrayOfElementsOfTheSameClass[] Array { get; set; }
        }

        public class CycleRoot
        {
            public Child1 Child1 { get; set; }
        }

        public class Child1
        {
            public IList<Child2> Child2IList { get; set; } = new List<Child2>();
            public List<Child2> Child2List { get; set; } = new List<Child2>();
            public Dictionary<string, Child2> Child2Dictionary { get; set; } = new Dictionary<string, Child2>();
            public Child2 Child2 { get; set; }
        }

        public interface IChild
        {
            Dictionary<string, Child1> Child1Dictionary { get; set; }
        }

        public class Child2 : IChild
        {
            // Add properties that cause a cycle (Root->Child1->Child2->Child1)
            public Child1 Child1 { get; set; }
            public IList<Child1> Child1IList { get; set; }
            public IList<Child1> Child1List { get; set; }
            public Dictionary<string, Child1> Child1Dictionary { get; set; }
        }

        public class CycleRootA
        {
            public ChildA Child1 { get; set; }
        }

        public class ChildA
        {
            public IList<ChildB> Child2IList { get; set; } = new List<ChildB>();
            public List<ChildB> Child2List { get; set; } = new List<ChildB>();
            public Dictionary<string, ChildB> Child2Dictionary { get; set; } = new Dictionary<string, ChildB>();
            public ChildB Child2 { get; set; }
        }

        public class ChildB
        {
            // Add properties that cause a cycle (Root->Child1->Child2->Child1)
            public ChildA Child1 { get; set; }
            public IList<ChildA> Child1IList { get; set; }
            public IList<ChildA> Child1List { get; set; }
            public Dictionary<string, ChildA> Child1Dictionary { get; set; }

            public IChild Child2 { get; set; }
        }

        [Fact]
        public static void MultiClassCycle()
        {
            CycleRoot root = new CycleRoot();
            root.Child1 = new Child1();
            root.Child1.Child2IList.Add(new Child2());
            root.Child1.Child2List.Add(new Child2());
            root.Child1.Child2Dictionary.Add("0", new Child2());
            root.Child1.Child2 = new Child2();
            root.Child1.Child2.Child1 = new Child1();

            // A cycle in just Types (not data) is allowed.
            string json = JsonSerializer.Generic.Utf16.Serialize(root);

            root = JsonSerializer.Generic.Utf16.Deserialize<CycleRoot>(json);
            Assert.NotNull(root.Child1);
            Assert.NotNull(root.Child1.Child2IList[0]);
            Assert.NotNull(root.Child1.Child2List[0]);
            Assert.NotNull(root.Child1.Child2Dictionary["0"]);
            Assert.NotNull(root.Child1.Child2);
            Assert.NotNull(root.Child1.Child2.Child1);

            // Round-trip
            string jsonRoundTrip = JsonSerializer.Generic.Utf16.Serialize(root);
            Assert.Equal(json, jsonRoundTrip);
        }

        [Fact]
        public static void IsPolymorphically()
        {
            Assert.False(JsonComplexSerializer.IsPolymorphically<TestClassWithCycle>());
            Assert.False(JsonComplexSerializer.IsPolymorphically<TestClassWithArrayOfElementsOfTheSameClass>());
            Assert.False(JsonComplexSerializer.IsPolymorphically<CycleRoot>());
            Assert.True(JsonComplexSerializer.IsPolymorphically<CycleRootA>());
        }
    }
}
