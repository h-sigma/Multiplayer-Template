using UnityEngine;

namespace Utility
{
    public static class VectorUtils
    {
        public static bool IsNan(this Vector3 vec3)
        {
            return float.IsNaN(vec3.x) || float.IsNaN(vec3.y) || float.IsNaN(vec3.z);
        }
    }
}