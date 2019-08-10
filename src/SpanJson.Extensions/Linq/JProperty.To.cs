using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonWriter = Newtonsoft.Json.JsonWriter;

namespace SpanJson.Linq
{
    partial class JProperty
    {
        /// <summary>Writes this token to a <see cref="NJsonWriter"/>.</summary>
        /// <param name="writer">A <see cref="NJsonWriter"/> into which this method will write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteTo(NJsonWriter writer, NJsonSerializer serializer)
        {
            writer.WritePropertyName(_name);

            JToken value = Value;
            if (value != null)
            {
                value.WriteTo(writer, serializer);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
