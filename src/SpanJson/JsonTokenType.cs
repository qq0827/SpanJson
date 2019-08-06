namespace SpanJson
{
    /// <summary>This enum defines the various JSON tokens that make up a JSON text and is used by
    /// the <see cref="T:JsonReader"/> when moving from one token to the next.
    /// The <see cref="T:JsonReader"/> starts at 'None' by default. The 'Comment' enum value
    /// is only ever reached in a specific <see cref="T:JsonReader"/> mode and is not
    /// reachable by default.</summary>
    public enum JsonTokenType : byte
    {
        /// <summary>Indicates that there is no value (as distinct from <see cref="Null"/>).</summary>
        /// <remarks>This is the default token type if no data has been read by the <see cref="T:JsonReader"/>.</remarks>
        None = 0,

        /// <summary>Indicates that the token type is the start of a JSON object.</summary>
        BeginObject = 1,

        /// <summary>Indicates that the token type is the end of a JSON object.</summary>
        EndObject = 2,

        /// <summary>Indicates that the token type is the start of a JSON array.</summary>
        BeginArray = 3,

        /// <summary>Indicates that the token type is the end of a JSON array.</summary>
        EndArray = 4,


        /// <summary>Indicates that the token type is a JSON property name.</summary>
        PropertyName = 5,
        /// <summary>Indicates that the token type is the comment string.</summary>
        Comment = 6,


        /// <summary>Indicates that the token type is a JSON string.</summary>
        String = 7,

        /// <summary>Indicates that the token type is a JSON number.</summary>
        Number = 8,

        /// <summary>Indicates that the token type is the JSON literal <c>true</c>.</summary>
        True = 9,

        /// <summary>Indicates that the token type is the JSON literal <c>false</c>.</summary>
        False = 10,

        /// <summary>Indicates that the token type is the JSON literal <c>null</c>.</summary>
        Null = 11,

        /// <summary>,</summary>
        ValueSeparator = 12,

        /// <summary>:</summary>
        NameSeparator = 13
    }
}