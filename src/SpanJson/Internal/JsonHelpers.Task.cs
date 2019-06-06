using System.Runtime.CompilerServices;
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
    }
}
