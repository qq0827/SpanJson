using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SpanJson.Internal
{
    static partial class JsonHelpers
    {
#if NET451
        public static readonly Task CompletedTask = Task.FromResult(0);
#else
        public static readonly Task CompletedTask = Task.CompletedTask;
#endif

        /// <summary>TBD</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCompletedSuccessfully(this Task task)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            return task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
#else
            return task.IsCompletedSuccessfully;
#endif
        }

        /// <summary>TBD</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCompletedSuccessfully<T>(this Task<T> task)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            return task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
#else
            return task.IsCompletedSuccessfully;
#endif
        }

        /// <summary>Returns an error task. The task is Completed, IsCanceled = False, IsFaulted = True</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task FromException(Exception exception)
        {
#if NET_4_5_GREATER
            return Task.FromException(exception);
#else
            return FromException<VoidTaskResult>(exception);
#endif
        }

        /// <summary>Returns an error task of the given type. The task is Completed, IsCanceled = False, IsFaulted = True</summary>
        /// <typeparam name="TResult"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> FromException<TResult>(Exception exception)
        {
#if NET_4_5_GREATER
            return Task.FromException<TResult>(exception);
#else
            //var tcs = new TaskCompletionSource<TResult>();
            //tcs.SetException(exception);
            //return tcs.Task;
            if (exception is AggregateException aggregateException)
            {
                var tcs = new TaskCompletionSource<TResult>();
                tcs.SetException(aggregateException.InnerExceptions);
                return tcs.Task;
            }
            else
            {
                var atmb = System.Runtime.CompilerServices.AsyncTaskMethodBuilder<TResult>.Create();
                atmb.SetException(exception);
                return atmb.Task;
            }
#endif
        }

        private struct VoidTaskResult { }
    }
}
