using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace SpanJson.Tests
{
    public class ConstructorAttributeTests
    {
        [Fact(Skip = "not yet supported")]
        public void TestQueryUtf8()
        {
            var query = new Query("MsSQL");
            query.Name = "SQL Server";
            query.Schema = "dbo";
            var serialized = JsonSerializer.Generic.Utf8.Serialize(query);
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<Query>(serialized);
            Assert.Equal(query.Name, deserialized.Name);
            Assert.Equal(query.Source, deserialized.Source);
            Assert.Equal(query.Schema, deserialized.Schema);
        }

        [Fact(Skip = "not yet supported")]
        public void TestQueryUtf16()
        {
            var query = new Query("PostgreSQL");
            query.Name = "PostgreSQL 11";
            query.Schema = "progres";
            var serialized = JsonSerializer.Generic.Utf16.Serialize(query);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<Query>(serialized);
            Assert.Equal(query.Name, deserialized.Name);
            Assert.Equal(query.Source, deserialized.Source);
            Assert.Equal(query.Schema, deserialized.Schema);
        }

        [Fact]
        public void TestQueryJsonNet()
        {
            var query = new Query("PostgreSQL");
            query.Name = "PostgreSQL 11";
            query.Schema = "progres";
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(query);
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<Query>(serialized);
            Assert.Equal(query.Name, deserialized.Name);
            Assert.Equal(query.Source, deserialized.Source);
            Assert.Equal(query.Schema, deserialized.Schema);
        }

        [Fact]
        public void TestQueryUtf8Json()
        {
            var query = new Query("PostgreSQL");
            query.Name = "PostgreSQL 11";
            query.Schema = "progres";
            var serialized = Utf8Json.JsonSerializer.Serialize(query);
            var deserialized = Utf8Json.JsonSerializer.Deserialize<Query>(serialized);
            Assert.Equal(query.Name, deserialized.Name);
            Assert.Equal(query.Source, deserialized.Source);
            Assert.Equal(query.Schema, deserialized.Schema);
        }

        public class Query
        {
            [JsonConstructor(nameof(Source))]
            public Query(string source)
            {
                Source = source;
            }

            public string Name { get; set; }

            public string Source { get; }

            public string Schema { get; set; }
        }

        public class SqlQuery : Query
        {
            public SqlQuery() : base("Sql")
            {
            }

            public string Template { get; set; }
            public bool ReturnDocuments { get; set; }
        }
    }
}
