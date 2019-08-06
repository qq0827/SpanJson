namespace SpanJson.Linq
{
    public delegate bool JValueCompareDelegate(JTokenType valueType, object objA, object objB, out int result);
}
