using System.Collections.Generic;

namespace SpanJson.Linq.JsonPath
{
    internal class FieldMultipleFilter : PathFilter
    {
        public List<string> Names { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current)
            {
                if (t is JObject o)
                {
                    foreach (string name in Names)
                    {
                        JToken v = o[name];

                        if (v != null)
                        {
                            yield return v;
                        }

                        if (errorWhenNoMatch)
                        {
                            ThrowHelper2.ThrowJsonException_Property_does_not_exist_on_JObject(name);
                        }
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        ThrowHelper2.ThrowJsonException_Property_not_valid_on(Names, t);
                    }
                }
            }
        }
    }
}