using System;

namespace SpanJson.Linq
{
    partial class JToken : ICloneable
    {
        object ICloneable.Clone()
        {
            return DeepClone();
        }

        /// <summary>Creates a new instance of the <see cref="JToken"/>. All child tokens are recursively cloned.</summary>
        /// <returns>A new instance of the <see cref="JToken"/>.</returns>
        public JToken DeepClone()
        {
            return CloneToken();
        }

        internal abstract JToken CloneToken();

        /// <summary>Compares the values of two tokens, including the values of all descendant tokens.</summary>
        /// <param name="t1">The first <see cref="JToken"/> to compare.</param>
        /// <param name="t2">The second <see cref="JToken"/> to compare.</param>
        /// <returns><c>true</c> if the tokens are equal; otherwise <c>false</c>.</returns>
        public static bool DeepEquals(JToken t1, JToken t2)
        {
            return (t1 == t2 || (t1 is object && t2 is object && t1.DeepEquals(t2)));
        }

        internal abstract bool DeepEquals(JToken node);
    }
}