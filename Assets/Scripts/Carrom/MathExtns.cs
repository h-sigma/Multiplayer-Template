using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Carrom
{
    public static class MathExtns
    {
        /// <summary>
        /// Maps value in range [min1, max1] to [min2, max2] without any clamping.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(this float value, float min1, float max1, float min2, float max2)
        {
            float t = (value - min1) / (max1 - min1);
            return min2 + t * (max2 - min2);
        }

        /// <summary>
        /// Maps value in range [min1, max1] to [min2, max2].
        /// Result will always lie in [min2, max2] regardless if 0 <= InverseLerp(value, min1, max1) <= 1 or not.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RemapClamped(this float value, float min1, float max1, float min2, float max2)
        {
            float t = (value - min1) / (max1 - min1);
            return Mathf.Clamp(min2 + t * (max2 - min2), min2, max2);
        }

        /// <summary>
        /// Maps value in range [min1, max1] to [0, 1].
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap01(this float value, float min1, float max1)
        {
            return value.RemapClamped(min1, max1, 0, 1);
        }
        
        /// <summary>
        /// Maps value in range [min1, max1] to [min2, max2] using modulation function.
        /// <example>
        ///     Let, modulate(t) = 2t
        ///     Remap (0.5, 0, 1, 2, 4, modulate) :-
        ///         t = (0.5 - 0) / (1 - 0) = 0.5   
        ///         modt = modulate(t) = 2 * t = 1.0
        ///         result = min2 + modt * (max2 - min2) = 2 + 1 * (4 - 2)
        ///     =>  result = 4
        /// </example>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(this float value, float min1, float max1, float min2, float max2,
            Func<float, float>               modulation)
        {
            float t    = (value - min1) / (max1 - min1);
            float modt = modulation(t);
            return min2 + (max2 - min2) * modt;
        }
        
        
    }
}