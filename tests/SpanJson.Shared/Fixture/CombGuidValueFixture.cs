using System;
using CuteAnt;

namespace SpanJson.Shared.Fixture
{
    public class CombGuidValueFixture : IValueFixture
    {
        public Type Type { get; } = typeof(CombGuid);

        public object Generate()
        {
            return CombGuid.NewComb();
        }
    }
}