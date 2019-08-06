using System.Collections.Generic;

namespace SpanJson.Linq
{
    partial class JProperty
    {
        /// <summary>Writes this token to a <see cref="Newtonsoft.Json.JsonWriter"/>.</summary>
        /// <param name="writer">A <see cref="Newtonsoft.Json.JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="Newtonsoft.Json.JsonConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(Newtonsoft.Json.JsonWriter writer, IList<Newtonsoft.Json.JsonConverter> converters)
        {
            writer.WritePropertyName(_name);

            JToken value = Value;
            if (value != null)
            {
                value.WriteTo(writer, converters);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
