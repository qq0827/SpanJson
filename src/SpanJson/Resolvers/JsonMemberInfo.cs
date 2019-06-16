using System;
using System.Reflection;
using SpanJson.Internal;

namespace SpanJson.Resolvers
{
    public class JsonMemberInfo
    {
        public JsonMemberInfo(string memberName, Type memberType, MethodInfo shouldSerialize, string name, JsonEncodedText escapedName,
            bool excludeNull, bool canRead, bool canWrite, Type customSerializer, object customSerializerArguments)
        {
            MemberName = memberName;
            MemberType = memberType;
            ShouldSerialize = shouldSerialize;
            Name = name;
            EscapedName = escapedName;
            ExcludeNull = excludeNull;
            CanRead = canRead;
            CanWrite = canWrite;
            CustomSerializer = customSerializer;
            CustomSerializerArguments = customSerializerArguments;
        }

        public string MemberName { get; }
        public Type MemberType { get; }
        public MethodInfo ShouldSerialize { get; }
        public string Name { get; }
        public JsonEncodedText EscapedName { get; }
        public bool ExcludeNull { get; }

        public Type CustomSerializer { get; }
        public object CustomSerializerArguments { get; }

        public bool CanRead { get; }
        public bool CanWrite { get; set; }
    }
}