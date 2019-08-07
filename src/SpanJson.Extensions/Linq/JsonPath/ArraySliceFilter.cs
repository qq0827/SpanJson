using System;
using System.Collections.Generic;

namespace SpanJson.Linq.JsonPath
{
    internal class ArraySliceFilter : PathFilter
    {
        public int? Start { get; set; }
        public int? End { get; set; }
        public int? Step { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            var step = Step;
            if (step.HasValue && 0u >= (uint)step.Value) { ThrowHelper2.ThrowJsonException_Step_cannot_be_zero(); }

            var start = Start;
            var end = End;
            foreach (JToken t in current)
            {
                if (t is JArray a)
                {
                    // set defaults for null arguments
                    int stepCount = step ?? 1;
                    int startIndex = start ?? ((stepCount > 0) ? 0 : a.Count - 1);
                    int stopIndex = end ?? ((stepCount > 0) ? a.Count : -1);

                    // start from the end of the list if start is negative
                    if (start.HasValue && (uint)start.Value > JsonSharedConstant.TooBigOrNegative)
                    {
                        startIndex = a.Count + startIndex;
                    }

                    // end from the start of the list if stop is negative
                    if (end.HasValue && (uint)end.Value > JsonSharedConstant.TooBigOrNegative)
                    {
                        stopIndex = a.Count + stopIndex;
                    }

                    // ensure indexes keep within collection bounds
                    startIndex = Math.Max(startIndex, (stepCount > 0) ? 0 : int.MinValue);
                    startIndex = Math.Min(startIndex, (stepCount > 0) ? a.Count : a.Count - 1);
                    stopIndex = Math.Max(stopIndex, -1);
                    stopIndex = Math.Min(stopIndex, a.Count);

                    bool positiveStep = (stepCount > 0);

                    if (IsValid(startIndex, stopIndex, positiveStep))
                    {
                        for (int i = startIndex; IsValid(i, stopIndex, positiveStep); i += stepCount)
                        {
                            yield return a[i];
                        }
                    }
                    else
                    {
                        if (errorWhenNoMatch)
                        {
                            ThrowHelper2.ThrowJsonException_Array_slice_of_to_returned_to_results(start, end);
                        }
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        ThrowHelper2.ThrowJsonException_Array_slice_is_not_valid_on(t);
                    }
                }
            }
        }

        private bool IsValid(int index, int stopIndex, bool positiveStep)
        {
            if (positiveStep)
            {
                return (index < stopIndex) ? true : false;
            }

            return (index > stopIndex) ? true : false;
        }
    }
}