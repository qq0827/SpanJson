using System.Collections.Generic;

namespace SpanJson.Linq.JsonPath
{
    internal class ScanMultipleFilter : PathFilter
    {
        private List<string> _names;

        public ScanMultipleFilter(List<string> names)
        {
            _names = names;
        }

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
                        foreach (string name in _names)
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