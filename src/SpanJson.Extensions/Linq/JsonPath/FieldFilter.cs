using System.Collections.Generic;

namespace SpanJson.Linq.JsonPath
{
    internal class FieldFilter : PathFilter
    {
        public string Name { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                if (t is JObject o)
                {
                    if (Name is object)
                    {
                        JToken v = o[Name];

                        if (v is object)
                        {
                            yield return v;
                        }
                        else if (errorWhenNoMatch)
                        {
                            ThrowHelper2.ThrowJsonException_Property_does_not_exist_on_JObject(Name);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, JToken> p in o)
                        {
                            yield return p.Value;
                        }
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        ThrowHelper2.ThrowJsonException_Property_not_valid_on(Name, t);
                    }
                }
            }
        }
    }
}