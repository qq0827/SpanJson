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
            if (value is object)
            {
                value.WriteTo(writer, serializer);
            }
            else
            {
                writer.WriteNull();
            }
        }

        /// <summary>Writes this token to a <see cref="Utf8JsonWriter"/>.</summary>
        /// <param name="writer">A <see cref="Utf8JsonWriter"/> into which this method will write.</param>
        public override void WriteTo(ref Utf8JsonWriter writer)
        {
            writer.WritePropertyName(_name);

            JToken value = Value;
            if (value is object)
            {
                value.WriteTo(ref writer);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
