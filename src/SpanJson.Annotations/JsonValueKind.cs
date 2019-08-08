// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// borrowed from https://github.com/dotnet/corefx/blob/8135319caa7e457ed61053ca1418313b88057b51/src/System.Text.Json/src/System/Text/Json/Document/JsonValueKind.cs

namespace SpanJson
{
    /// <summary>
    ///   Specifies the data type of a JSON value.
    /// </summary>
    public enum JsonValueKind : byte
    {
        /// <summary>
        ///   Indicates that there is no value (as distinct from <see cref="Null"/>).
        /// </summary>
        Undefined,

        /// <summary>
        ///   Indicates that a value is a JSON object.
        /// </summary>
        Object,

        /// <summary>
        ///   Indicates that a value is a JSON array.
        /// </summary>
        Array,

        /// <summary>
        ///   Indicates that a value is a JSON string.
        /// </summary>
        String,

        /// <summary>
        ///   Indicates that a value is a JSON number.
        /// </summary>
        Number,

        /// <summary>
        ///   Indicates that a value is the JSON value <c>true</c>.
        /// </summary>
        True,

        /// <summary>
        ///   Indicates that a value is the JSON value <c>false</c>.
        /// </summary>
        False,

        /// <summary>
        ///   Indicates that a value is the JSON value <c>null</c>.
        /// </summary>
        Null,
    }
}
