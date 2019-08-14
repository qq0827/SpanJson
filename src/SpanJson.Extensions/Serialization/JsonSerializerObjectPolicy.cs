// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using CuteAnt.Pool;
using Newtonsoft.Json;

namespace SpanJson.Serialization
{
    /// <summary><see cref="IPooledObjectPolicy{T}"/> for <see cref="Newtonsoft.Json.JsonSerializer"/>.</summary>
    public class JsonSerializerObjectPolicy : IPooledObjectPolicy<Newtonsoft.Json.JsonSerializer>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>Initializes a new instance of <see cref="JsonSerializerObjectPolicy"/>.</summary>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings"/> used to instantiate
        /// <see cref="Newtonsoft.Json.JsonSerializer"/> instances.</param>
        public JsonSerializerObjectPolicy(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        /// <inheritdoc />
        public Newtonsoft.Json.JsonSerializer Create() => Newtonsoft.Json.JsonSerializer.Create(_serializerSettings); // CreateDefault

        /// <inheritdoc />
        public Newtonsoft.Json.JsonSerializer PreGetting(Newtonsoft.Json.JsonSerializer serializer) => serializer;

        /// <inheritdoc />
        public bool Return(Newtonsoft.Json.JsonSerializer serializer) => serializer is object;
    }
}