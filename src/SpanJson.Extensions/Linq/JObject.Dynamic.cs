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

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    partial class JObject
    {
        /// <summary>Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.</summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> to bind this object.</returns>
        protected override DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<JObject>(parameter, this, new JObjectDynamicProxy());
        }

        private class JObjectDynamicProxy : DynamicProxy<JObject>
        {
            public override bool TryGetMember(JObject instance, GetMemberBinder binder, out object result)
            {
                // result can be null
                result = instance[binder.Name];
                return true;
            }

            public override bool TrySetMember(JObject instance, SetMemberBinder binder, object value)
            {
                // this can throw an error if value isn't a valid for a JValue
                if (!(value is JToken v))
                {
                    v = new JValue(value);
                }

                instance[binder.Name] = v;
                return true;
            }

            public override IEnumerable<string> GetDynamicMemberNames(JObject instance)
            {
                return instance.Properties().Select(p => p.Name);
            }
        }
    }
}
