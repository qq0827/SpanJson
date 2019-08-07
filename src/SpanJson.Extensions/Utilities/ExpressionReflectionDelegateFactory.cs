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
using System.Linq;
using System.Text;
using System;
using System.Linq.Expressions;
using System.Reflection;
using CuteAnt;
using CuteAnt.Reflection;

namespace SpanJson.Utilities
{
    internal class ExpressionReflectionDelegateFactory : ReflectionDelegateFactory
    {
        private static readonly ExpressionReflectionDelegateFactory _instance = new ExpressionReflectionDelegateFactory();

        internal static ReflectionDelegateFactory Instance => _instance;

        public override CtorInvoker<object> CreateParameterizedConstructor(MethodBase method)
        {
            if (null == method) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method); }

            Type type = typeof(object);

            ParameterExpression argsParameterExpression = Expression.Parameter(typeof(object[]), "args");

            Expression callExpression = BuildMethodCall(method, type, null, argsParameterExpression);

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(CtorInvoker<object>), callExpression, argsParameterExpression);

            CtorInvoker<object> compiled = (CtorInvoker<object>)lambdaExpression.Compile();
            return compiled;
        }

        public override MethodCaller<T, object> CreateMethodCall<T>(MethodBase method)
        {
            if (null == method) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method); }

            Type type = typeof(object);

            ParameterExpression targetParameterExpression = Expression.Parameter(type, "target");
            ParameterExpression argsParameterExpression = Expression.Parameter(typeof(object[]), "args");

            Expression callExpression = BuildMethodCall(method, type, targetParameterExpression, argsParameterExpression);

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(MethodCaller<T, object>), callExpression, targetParameterExpression, argsParameterExpression);

            MethodCaller<T, object> compiled = (MethodCaller<T, object>)lambdaExpression.Compile();
            return compiled;
        }

        private class ByRefParameter
        {
            public Expression Value;
            public ParameterExpression Variable;
            public bool IsOut;
        }

        private Expression BuildMethodCall(MethodBase method, Type type, ParameterExpression targetParameterExpression, ParameterExpression argsParameterExpression)
        {
            ParameterInfo[] parametersInfo = method.GetParameters();

            Expression[] argsExpression;
            IList<ByRefParameter> refParameterMap;
            if (0u >= (uint)parametersInfo.Length)
            {
                argsExpression = EmptyArray<Expression>.Instance;
                refParameterMap = EmptyArray<ByRefParameter>.Instance;
            }
            else
            {
                argsExpression = new Expression[parametersInfo.Length];
                refParameterMap = new List<ByRefParameter>();

                for (int i = 0; i < parametersInfo.Length; i++)
                {
                    ParameterInfo parameter = parametersInfo[i];
                    Type parameterType = parameter.ParameterType;
                    bool isByRef = false;
                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                        isByRef = true;
                    }

                    Expression indexExpression = Expression.Constant(i);

                    Expression paramAccessorExpression = Expression.ArrayIndex(argsParameterExpression, indexExpression);

                    Expression argExpression = EnsureCastExpression(paramAccessorExpression, parameterType, !isByRef);

                    if (isByRef)
                    {
                        ParameterExpression variable = Expression.Variable(parameterType);
                        refParameterMap.Add(new ByRefParameter {Value = argExpression, Variable = variable, IsOut = parameter.IsOut});

                        argExpression = variable;
                    }

                    argsExpression[i] = argExpression;
                }
            }

            Expression callExpression;
            if (method.IsConstructor)
            {
                callExpression = Expression.New((ConstructorInfo)method, argsExpression);
            }
            else if (method.IsStatic)
            {
                callExpression = Expression.Call((MethodInfo)method, argsExpression);
            }
            else
            {
                Expression readParameter = EnsureCastExpression(targetParameterExpression, method.DeclaringType);

                callExpression = Expression.Call(readParameter, (MethodInfo)method, argsExpression);
            }

            if (method is MethodInfo m)
            {
                if (m.ReturnType != typeof(void))
                {
                    callExpression = EnsureCastExpression(callExpression, type);
                }
                else
                {
                    callExpression = Expression.Block(callExpression, Expression.Constant(null));
                }
            }
            else
            {
                callExpression = EnsureCastExpression(callExpression, type);
            }

            if (refParameterMap.Count > 0)
            {
                IList<ParameterExpression> variableExpressions = new List<ParameterExpression>();
                IList<Expression> bodyExpressions = new List<Expression>();
                foreach (ByRefParameter p in refParameterMap)
                {
                    if (!p.IsOut)
                    {
                        bodyExpressions.Add(Expression.Assign(p.Variable, p.Value));
                    }

                    variableExpressions.Add(p.Variable);
                }

                bodyExpressions.Add(callExpression);

                callExpression = Expression.Block(variableExpressions, bodyExpressions);
            }

            return callExpression;
        }

        public override Func<T> CreateDefaultConstructor<T>(Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            // avoid error from expressions compiler because of abstract class
            if (type.IsAbstract)
            {
                return () => (T)Activator.CreateInstance(type);
            }

            try
            {
                Type resultType = typeof(T);

                Expression expression = Expression.New(type);

                expression = EnsureCastExpression(expression, resultType);

                LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T>), expression);

                Func<T> compiled = (Func<T>)lambdaExpression.Compile();
                return compiled;
            }
            catch
            {
                // an error can be thrown if constructor is not valid on Win8
                // will have INVOCATION_FLAGS_NON_W8P_FX_API invocation flag
                return () => (T)Activator.CreateInstance(type);
            }
        }

        public override Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
        {
            if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }

            Type instanceType = typeof(T);
            Type resultType = typeof(object);

            ParameterExpression parameterExpression = Expression.Parameter(instanceType, "instance");
            Expression resultExpression;

            MethodInfo getMethod = propertyInfo.GetGetMethod(true);

            if (getMethod.IsStatic)
            {
                resultExpression = Expression.MakeMemberAccess(null, propertyInfo);
            }
            else
            {
                Expression readParameter = EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType);

                resultExpression = Expression.MakeMemberAccess(readParameter, propertyInfo);
            }

            resultExpression = EnsureCastExpression(resultExpression, resultType);

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T, object>), resultExpression, parameterExpression);

            Func<T, object> compiled = (Func<T, object>)lambdaExpression.Compile();
            return compiled;
        }

        public override Func<T, object> CreateGet<T>(FieldInfo fieldInfo)
        {
            if (null == fieldInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fieldInfo); }

            ParameterExpression sourceParameter = Expression.Parameter(typeof(T), "source");

            Expression fieldExpression;
            if (fieldInfo.IsStatic)
            {
                fieldExpression = Expression.Field(null, fieldInfo);
            }
            else
            {
                Expression sourceExpression = EnsureCastExpression(sourceParameter, fieldInfo.DeclaringType);

                fieldExpression = Expression.Field(sourceExpression, fieldInfo);
            }

            fieldExpression = EnsureCastExpression(fieldExpression, typeof(object));

            Func<T, object> compiled = Expression.Lambda<Func<T, object>>(fieldExpression, sourceParameter).Compile();
            return compiled;
        }

        public override Action<T, object> CreateSet<T>(FieldInfo fieldInfo)
        {
            if (null == fieldInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fieldInfo); }

            // use reflection for structs
            // expression doesn't correctly set value
            if (fieldInfo.DeclaringType.IsValueType || fieldInfo.IsInitOnly)
            {
                return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(fieldInfo);
            }

            ParameterExpression sourceParameterExpression = Expression.Parameter(typeof(T), "source");
            ParameterExpression valueParameterExpression = Expression.Parameter(typeof(object), "value");

            Expression fieldExpression;
            if (fieldInfo.IsStatic)
            {
                fieldExpression = Expression.Field(null, fieldInfo);
            }
            else
            {
                Expression sourceExpression = EnsureCastExpression(sourceParameterExpression, fieldInfo.DeclaringType);

                fieldExpression = Expression.Field(sourceExpression, fieldInfo);
            }

            Expression valueExpression = EnsureCastExpression(valueParameterExpression, fieldExpression.Type);

            BinaryExpression assignExpression = Expression.Assign(fieldExpression, valueExpression);

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<T, object>), assignExpression, sourceParameterExpression, valueParameterExpression);

            Action<T, object> compiled = (Action<T, object>)lambdaExpression.Compile();
            return compiled;
        }

        public override Action<T, object> CreateSet<T>(PropertyInfo propertyInfo)
        {
            if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }

            // use reflection for structs
            // expression doesn't correctly set value
            if (propertyInfo.DeclaringType.IsValueType)
            {
                return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(propertyInfo);
            }

            Type instanceType = typeof(T);
            Type valueType = typeof(object);

            ParameterExpression instanceParameter = Expression.Parameter(instanceType, "instance");

            ParameterExpression valueParameter = Expression.Parameter(valueType, "value");
            Expression readValueParameter = EnsureCastExpression(valueParameter, propertyInfo.PropertyType);

            MethodInfo setMethod = propertyInfo.GetSetMethod(true);

            Expression setExpression;
            if (setMethod.IsStatic)
            {
                setExpression = Expression.Call(setMethod, readValueParameter);
            }
            else
            {
                Expression readInstanceParameter = EnsureCastExpression(instanceParameter, propertyInfo.DeclaringType);

                setExpression = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
            }

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<T, object>), setExpression, instanceParameter, valueParameter);

            Action<T, object> compiled = (Action<T, object>)lambdaExpression.Compile();
            return compiled;
        }
        
        private Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
        {
            Type expressionType = expression.Type;
            
            // check if a cast or conversion is required
            if (expressionType == targetType || (!expressionType.IsValueType && targetType.IsAssignableFrom(expressionType)))
            {
                return expression;
            }

            if (targetType.IsValueType)
            {
                Expression convert = Expression.Unbox(expression, targetType);

                if (allowWidening && targetType.IsPrimitive)
                {
                    MethodInfo toTargetTypeMethod = typeof(Convert)
                        .GetMethod("To" + targetType.Name, new[] { typeof(object) });

                    if (toTargetTypeMethod != null)
                    {
                        convert = Expression.Condition(
                            Expression.TypeIs(expression, targetType),
                            convert,
                            Expression.Call(toTargetTypeMethod, expression));
                    }
                }
                
                return Expression.Condition(
                    Expression.Equal(expression, Expression.Constant(null, typeof(object))),
                    Expression.Default(targetType), 
                    convert);
            }

            return Expression.Convert(expression, targetType);
        }
    }
}
