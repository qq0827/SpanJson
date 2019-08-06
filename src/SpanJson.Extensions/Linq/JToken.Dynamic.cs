using System.Dynamic;
using System.Linq.Expressions;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    partial class JToken : IDynamicMetaObjectProvider
    {
        /// <summary>Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.</summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> to bind this object.</returns>
        protected virtual DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<JToken>(parameter, this, new DynamicProxy<JToken>());
        }

        /// <summary>Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.</summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> to bind this object.</returns>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return GetMetaObject(parameter);
        }
    }
}