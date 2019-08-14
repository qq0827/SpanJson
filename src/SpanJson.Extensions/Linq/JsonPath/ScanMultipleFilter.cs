using System.Collections.Generic;

namespace SpanJson.Linq.JsonPath
{
    internal class ScanMultipleFilter : PathFilter
    {
        public List<string> Names { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken c in current)
            {
                JToken value = c;

                while (true)
                {
                    JContainer container = value as JContainer;

                    value = GetNextScanValue(c, container, value);
                    if (value is null)
                    {
                        break;
                    }

                    if (value is JProperty property)
                    {
                        foreach (string name in Names)
                        {
                            if (property.Name == name)
                            {
                                yield return property.Value;
                            }
                        }
                    }

                }
            }
        }
    }
}