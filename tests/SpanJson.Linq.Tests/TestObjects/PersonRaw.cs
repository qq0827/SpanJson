#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Runtime.Serialization;
using SpanJson.Linq;

namespace SpanJson.Tests
{
    public class PersonRaw
    {
        private Guid _internalId;
        private string _firstName;
        private string _lastName;
        private JRaw _rawContent;

        [Newtonsoft.Json.JsonIgnore]
        [IgnoreDataMember]
        public Guid InternalId
        {
            get { return _internalId; }
            set { _internalId = value; }
        }

        [Newtonsoft.Json.JsonProperty("first_name")]
        [DataMember(Name = "first_name")]
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public JRaw RawContent
        {
            get { return _rawContent; }
            set { _rawContent = value; }
        }

        [Newtonsoft.Json.JsonProperty("last_name")]
        [DataMember(Name = "last_name")]
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }
    }
}