using System.Collections;
using System.Collections.Generic;

namespace SpanJson.Linq
{
    partial class JToken : IJEnumerable<JToken>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<JToken>)this).GetEnumerator();
        }

        IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator()
        {
            return Children().GetEnumerator();
        }

        IJEnumerable<JToken> IJEnumerable<JToken>.this[object key] => this[key];

        /// <summary>Returns a collection of the ancestor tokens of this token.</summary>
        /// <returns>A collection of the ancestor tokens of this token.</returns>
        public IEnumerable<JToken> Ancestors()
        {
            return GetAncestors(false);
        }

        /// <summary>Returns a collection of tokens that contain this token, and the ancestors of this token.</summary>
        /// <returns>A collection of tokens that contain this token, and the ancestors of this token.</returns>
        public IEnumerable<JToken> AncestorsAndSelf()
        {
            return GetAncestors(true);
        }

        internal IEnumerable<JToken> GetAncestors(bool self)
        {
            for (JToken current = self ? this : Parent; current is object; current = current.Parent)
            {
                yield return current;
            }
        }

        /// <summary>Returns a collection of the sibling tokens after this token, in document order.</summary>
        /// <returns>A collection of the sibling tokens after this tokens, in document order.</returns>
        public IEnumerable<JToken> AfterSelf()
        {
            if (Parent is null)
            {
                yield break;
            }

            for (JToken o = Next; o is object; o = o.Next)
            {
                yield return o;
            }
        }

        /// <summary>Returns a collection of the sibling tokens before this token, in document order.</summary>
        /// <returns>A collection of the sibling tokens before this token, in document order.</returns>
        public IEnumerable<JToken> BeforeSelf()
        {
            for (JToken o = Parent.First; o != this; o = o.Next)
            {
                yield return o;
            }
        }
    }
}