using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utility
{
    public static class ComponentUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetComponentIfNull<T>(this GameObject gameObject, ref T component) where T : Component
        {
            if (component == null) component = gameObject.GetComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetComponentInChildrenIfNull<T>(this GameObject gameObject, ref T component)
            where T : Component
        {
            if (component == null) component = gameObject.GetComponentInChildren<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetComponentInParentIfNull<T>(this GameObject gameObject, ref T component)
            where T : Component
        {
            if (component == null) component = gameObject.GetComponentInParent<T>();
        }
        
        
    }
}