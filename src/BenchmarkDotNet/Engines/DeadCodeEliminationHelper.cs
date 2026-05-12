using JetBrains.Annotations;
using System.Runtime.CompilerServices;

namespace BenchmarkDotNet.Engines
{
    public static class DeadCodeEliminationHelper
    {
        /// <summary>
        /// This method can't get inlined, so any value send to it
        /// will not get eliminated by the dead code elimination
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UsedImplicitly] // Used in generated benchmarks
        public static void KeepAliveWithoutBoxing<T>(T value)
#if NET9_0_OR_GREATER
            where T : allows ref struct
#endif
        { }

        /// <summary>
        /// This method can't get inlined, so any value send to it
        /// will not get eliminated by the dead code elimination
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UsedImplicitly] // Used in generated benchmarks
        public static void KeepAliveWithoutBoxing<T>(ref T value)
#if NET9_0_OR_GREATER
            where T : allows ref struct
#endif
        { }

        /// <summary>
        /// This method can't get inlined, so any value send to it
        /// will not get eliminated by the dead code elimination
        /// it's not called KeepAliveWithoutBoxing because compiler would not be able to diff `ref` and `in`
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UsedImplicitly] // Used in generated benchmarks
        public static void KeepAliveWithoutBoxingReadonly<T>(in T value)
#if NET9_0_OR_GREATER
            where T : allows ref struct
#endif
        { }
    }
}