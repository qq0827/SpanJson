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
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using SpanJson.Linq.JsonPath;
using SpanJson.Serialization;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    /// <summary>Represents an abstract JSON token.</summary>
    public abstract partial class JToken : IDynamicMetaObjectProvider
    {
        private static JTokenEqualityComparer _equalityComparer;

        private JContainer _parent;
        private JToken _previous;
        private JToken _next;
        private object _annotations;

        /// <summary>Gets a comparer that can compare two tokens for value equality.</summary>
        /// <value>A <see cref="JTokenEqualityComparer"/> that can compare two nodes for value equality.</value>
        public static JTokenEqualityComparer EqualityComparer
        {
            get
            {
                if (_equalityComparer is null)
                {
                    _equalityComparer = new JTokenEqualityComparer();
                }

                return _equalityComparer;
            }
        }

        /// <summary>Gets or sets the parent.</summary>
        /// <value>The parent.</value>
        public JContainer Parent
        {
            [DebuggerStepThrough]
            get { return _parent; }
            internal set { _parent = value; }
        }

        /// <summary>Gets the root <see cref="JToken"/> of this <see cref="JToken"/>.</summary>
        /// <value>The root <see cref="JToken"/> of this <see cref="JToken"/>.</value>
        public JToken Root
        {
            get
            {
                JContainer parent = Parent;
                if (parent is null)
                {
                    return this;
                }

                while (parent.Parent is object)
                {
                    parent = parent.Parent;
                }

                return parent;
            }
        }

        /// <summary>Gets the node type for this <see cref="JToken"/>.</summary>
        /// <value>The type.</value>
        public abstract JTokenType Type { get; }

        /// <summary>Gets a value indicating whether this token has child tokens.</summary>
        /// <value><c>true</c> if this token has child values; otherwise, <c>false</c>.</value>
        public abstract bool HasValues { get; }

        /// <summary>Gets the next sibling token of this node.</summary>
        /// <value>The <see cref="JToken"/> that contains the next sibling token.</value>
        public JToken Next
        {
            get => _next;
            internal set => _next = value;
        }

        /// <summary>Gets the previous sibling token of this node.</summary>
        /// <value>The <see cref="JToken"/> that contains the previous sibling token.</value>
        public JToken Previous
        {
            get => _previous;
            internal set => _previous = value;
        }

        /// <summary>Gets the path of the JSON token.</summary>
        public string Path
        {
            get
            {
                if (Parent is null) { return string.Empty; }

                List<JsonPosition> positions = new List<JsonPosition>();
                JToken previous = null;
                for (JToken current = this; current is object; current = current.Parent)
                {
                    switch (current.Type)
                    {
                        case JTokenType.Property:
                            JProperty property = (JProperty)current;
                            positions.Add(new JsonPosition(JsonContainerType.Object) { PropertyName = property.Name });
                            break;
                        case JTokenType.Array:
                            //case JTokenType.Constructor:
                            if (previous is object)
                            {
                                int index = ((IList<JToken>)current).IndexOf(previous);

                                positions.Add(new JsonPosition(JsonContainerType.Array) { Position = index });
                            }
                            break;
                    }

                    previous = current;
                }

                positions.FastReverse();

                return JsonPosition.BuildPath(positions, null);
            }
        }

        internal JToken() { }

        public static bool IsNull(JToken token)
        {
            if (token.Type == JTokenType.Null) { return true; }

            if (token is JValue v && v.Value is null) { return true; }

            return false;
        }

        /// <summary>Adds the specified content immediately after this token.</summary>
        /// <param name="content">A content object that contains simple content or a collection of content objects to be added after this token.</param>
        public void AddAfterSelf(object content)
        {
            if (_parent is null) { ThrowHelper2.ThrowInvalidOperationException_The_parent_is_missing(); }

            int index = _parent.IndexOfItem(this);
            _parent.AddInternal(index + 1, content, false);
        }

        /// <summary>Adds the specified content immediately before this token.</summary>
        /// <param name="content">A content object that contains simple content or a collection of content objects to be added before this token.</param>
        public void AddBeforeSelf(object content)
        {
            if (_parent is null) { ThrowHelper2.ThrowInvalidOperationException_The_parent_is_missing(); }

            int index = _parent.IndexOfItem(this);
            _parent.AddInternal(index, content, false);
        }

        /// <summary>Gets the <see cref="JToken"/> with the specified key.</summary>
        /// <value>The <see cref="JToken"/> with the specified key.</value>
        public virtual JToken this[object key]
        {
            get => throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot access child value on {0}.", GetType()));
            set => throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot set child value on {0}.", GetType()));
        }

        /// <summary>Gets the <see cref="JToken"/> with the specified key converted to the specified type.</summary>
        /// <typeparam name="T">The type to convert the token to.</typeparam>
        /// <param name="key">The token key.</param>
        /// <returns>The converted token value.</returns>
        public virtual T Value<T>(object key)
        {
            JToken token = this[key];

            // null check to fix MonoTouch issue - https://github.com/dolbz/Newtonsoft.Json/commit/a24e3062846b30ee505f3271ac08862bb471b822
            return token is null ? default : Extensions.Convert<JToken, T>(token);
        }

        /// <summary>Get the first child token of this token.</summary>
        /// <value>A <see cref="JToken"/> containing the first child token of the <see cref="JToken"/>.</value>
        public virtual JToken First => throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot access child value on {0}.", GetType()));

        /// <summary>Get the last child token of this token.</summary>
        /// <value>A <see cref="JToken"/> containing the last child token of the <see cref="JToken"/>.</value>
        public virtual JToken Last => throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot access child value on {0}.", GetType()));

        /// <summary>Returns a collection of the child tokens of this token, in document order.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the child tokens of this <see cref="JToken"/>, in document order.</returns>
        public virtual JEnumerable<JToken> Children()
        {
            return JEnumerable<JToken>.Empty;
        }

        /// <summary>Returns a collection of the child tokens of this token, in document order, filtered by the specified type.</summary>
        /// <typeparam name="T">The type to filter the child tokens on.</typeparam>
        /// <returns>A <see cref="JEnumerable{T}"/> containing the child tokens of this <see cref="JToken"/>, in document order.</returns>
        public JEnumerable<T> Children<T>() where T : JToken
        {
            return new JEnumerable<T>(Children().OfType<T>());
        }

        /// <summary>Returns a collection of the child values of this token, in document order.</summary>
        /// <typeparam name="T">The type to convert the values to.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="JToken"/>, in document order.</returns>
        public virtual IEnumerable<T> Values<T>()
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot access child value on {0}.", GetType()));
        }

        /// <summary>Removes this token from its parent.</summary>
        public void Remove()
        {
            if (_parent is null) { ThrowHelper2.ThrowInvalidOperationException_The_parent_is_missing(); }

            _parent.RemoveItem(this);
        }

        /// <summary>Replaces this token with the specified token.</summary>
        /// <param name="value">The value.</param>
        public void Replace(JToken value)
        {
            if (_parent is null) { ThrowHelper2.ThrowInvalidOperationException_The_parent_is_missing(); }

            _parent.ReplaceItem(this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static JValue EnsureValue(JToken value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            if (value is JProperty property)
            {
                value = property.Value;
            }

            JValue v = value as JValue;

            return v;
        }

        internal static string GetType(JToken token)
        {
            if (token is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.token); }

            if (token is JProperty p)
            {
                token = p.Value;
            }

            return token.Type.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ValidateToken(JToken o, HashSet<JTokenType> validTypes, bool nullable)
        {
            return validTypes.Contains(o.Type) || (nullable && (o.Type == JTokenType.Null /*|| o.Type == JTokenType.Undefined*/)); // 屏蔽针对 JTokenType.Undefined 的判断
        }

        internal abstract int GetDeepHashCode();

        /// <summary>Selects a <see cref="JToken"/> using a JPath expression. Selects the token that matches the object path.</summary>
        /// <param name="path">A <see cref="String"/> that contains a JPath expression.</param>
        /// <returns>A <see cref="JToken"/>, or <c>null</c>.</returns>
        public JToken SelectToken(string path)
        {
            return SelectToken(path, false);
        }

        /// <summary>Selects a <see cref="JToken"/> using a JPath expression. Selects the token that matches the object path.</summary>
        /// <param name="path">A <see cref="String"/> that contains a JPath expression.</param>
        /// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
        /// <returns>A <see cref="JToken"/>.</returns>
        public JToken SelectToken(string path, bool errorWhenNoMatch)
        {
            JPath p = new JPath(path);

            JToken token = null;
            foreach (JToken t in p.Evaluate(this, this, errorWhenNoMatch))
            {
                if (token is object)
                {
                    ThrowHelper2.ThrowJsonException_Path_returned_multiple_tokens();
                }

                token = t;
            }

            return token;
        }

        /// <summary>Selects a collection of elements using a JPath expression.</summary>
        /// <param name="path">A <see cref="String"/> that contains a JPath expression.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> that contains the selected elements.</returns>
        public IEnumerable<JToken> SelectTokens(string path)
        {
            return SelectTokens(path, false);
        }

        /// <summary>Selects a collection of elements using a JPath expression.</summary>
        /// <param name="path">A <see cref="String"/> that contains a JPath expression.</param>
        /// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> that contains the selected elements.</returns>
        public IEnumerable<JToken> SelectTokens(string path, bool errorWhenNoMatch)
        {
            JPath p = new JPath(path);
            return p.Evaluate(this, this, errorWhenNoMatch);
        }

        /// <summary>Adds an object to the annotation list of this <see cref="JToken"/>.</summary>
        /// <param name="annotation">The annotation to add.</param>
        public void AddAnnotation(object annotation)
        {
            if (annotation is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.annotation); }

            switch (_annotations)
            {
                case null:
                    _annotations = (annotation is object[]) ? new[] { annotation } : annotation;
                    break;

                case object[] annotations:
                    int index = 0;
                    while ((uint)index < (uint)annotations.Length && annotations[index] is object)
                    {
                        index++;
                    }
                    if (index == annotations.Length)
                    {
                        Array.Resize(ref annotations, index * 2);
                        _annotations = annotations;
                    }
                    annotations[index] = annotation;
                    break;

                default:
                    _annotations = new[] { _annotations, annotation };
                    break;
            }
        }

        /// <summary>Get the first annotation object of the specified type from this <see cref="JToken"/>.</summary>
        /// <typeparam name="T">The type of the annotation to retrieve.</typeparam>
        /// <returns>The first annotation object that matches the specified type, or <c>null</c> if no annotation is of the specified type.</returns>
        public T Annotation<T>() where T : class
        {
            switch (_annotations)
            {
                case null:
                    return default;

                case object[] annotations:
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        object annotation = annotations[i];

                        if (annotation is null) { break; }
                        if (annotation is T local) { return local; }
                    }
                    return default;

                default:
                    return _annotations as T;
            }
        }

        /// <summary>Gets the first annotation object of the specified type from this <see cref="JToken"/>.</summary>
        /// <param name="type">The <see cref="Type"/> of the annotation to retrieve.</param>
        /// <returns>The first annotation object that matches the specified type, or <c>null</c> if no annotation is of the specified type.</returns>
        public object Annotation(Type type)
        {
            if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            switch (_annotations)
            {
                case null:
                    return null;

                case object[] annotations:
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        object o = annotations[i];

                        if (o is null) { break; }
                        if (type.IsInstanceOfType(o)) { return o; }
                    }
                    return null;

                default:
                    if (type.IsInstanceOfType(_annotations))
                    {
                        return _annotations;
                    }
                    return null;
            }
        }

        /// <summary>Gets a collection of annotations of the specified type for this <see cref="JToken"/>.</summary>
        /// <typeparam name="T">The type of the annotations to retrieve.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the annotations for this <see cref="JToken"/>.</returns>
        public IEnumerable<T> Annotations<T>() where T : class
        {
            switch (_annotations)
            {
                case null:
                    yield break;

                case object[] annotations:
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        object o = annotations[i];
                        if (o is null) { break; }

                        if (o is T casted)
                        {
                            yield return casted;
                        }
                    }
                    yield break;

                case T annotation:
                    yield return annotation;
                    break;

                default:
                    yield break;
            }
        }

        /// <summary>Gets a collection of annotations of the specified type for this <see cref="JToken"/>.</summary>
        /// <param name="type">The <see cref="Type"/> of the annotations to retrieve.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Object"/> that contains the annotations that match the specified type for this <see cref="JToken"/>.</returns>
        public IEnumerable<object> Annotations(Type type)
        {
            if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            switch (_annotations)
            {
                case null:
                    yield break;

                case object[] annotations:
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        object o = annotations[i];
                        if (o is null) { break; }

                        if (type.IsInstanceOfType(o))
                        {
                            yield return o;
                        }
                    }
                    yield break;

                default:
                    if (type.IsInstanceOfType(_annotations))
                    {
                        yield return _annotations;
                    }
                    yield break;
            }
        }

        /// <summary>Removes the annotations of the specified type from this <see cref="JToken"/>.</summary>
        /// <typeparam name="T">The type of annotations to remove.</typeparam>
        public void RemoveAnnotations<T>() where T : class
        {
            switch (_annotations)
            {
                case null:
                    break;

                case object[] annotations:
                    int index = 0;
                    int keepCount = 0;
                    while ((uint)index < (uint)annotations.Length)
                    {
                        object obj2 = annotations[index];
                        if (obj2 is null)
                        {
                            break;
                        }

                        if (!(obj2 is T))
                        {
                            annotations[keepCount++] = obj2;
                        }

                        index++;
                    }

                    if (keepCount != 0)
                    {
                        while (keepCount < index)
                        {
                            annotations[keepCount++] = null;
                        }
                    }
                    else
                    {
                        _annotations = null;
                    }
                    break;

                case T _:
                    _annotations = null;
                    break;
            }
        }

        /// <summary>Removes the annotations of the specified type from this <see cref="JToken"/>.</summary>
        /// <param name="type">The <see cref="Type"/> of annotations to remove.</param>
        public void RemoveAnnotations(Type type)
        {
            if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            switch (_annotations)
            {
                case null:
                    break;

                case object[] annotations:
                    int index = 0;
                    int keepCount = 0;
                    while ((uint)index < (uint)annotations.Length)
                    {
                        object o = annotations[index];
                        if (o is null) { break; }

                        if (!type.IsInstanceOfType(o))
                        {
                            annotations[keepCount++] = o;
                        }

                        index++;
                    }

                    if (keepCount != 0)
                    {
                        while (keepCount < index)
                        {
                            annotations[keepCount++] = null;
                        }
                    }
                    else
                    {
                        _annotations = null;
                    }
                    break;

                default:
                    if (type.IsInstanceOfType(_annotations))
                    {
                        _annotations = null;
                    }
                    break;
            }
        }
    }
}