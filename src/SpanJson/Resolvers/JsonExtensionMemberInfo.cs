using System;

namespace SpanJson.Resolvers
{
    public class JsonExtensionMemberInfo
    {
        public JsonExtensionMemberInfo(string memberName, Type memberType, bool excludeNulls)
        {
            MemberName = memberName;
            MemberType = memberType;
            ExcludeNulls = excludeNulls;
        }

        public string MemberName { get; }
        public Type MemberType { get; }
        public bool ExcludeNulls { get; }
    }
}
