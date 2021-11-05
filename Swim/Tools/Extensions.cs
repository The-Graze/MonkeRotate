using UnityEngine;

namespace MonkeSwim.Tools
{
    public static class Extensions
    {

        public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
        {
            value.ClampThis(min, max);
            return value;
        }
        public static void ClampThis(this ref Vector3 value, Vector3 min, Vector3 max)
        {
            value.x = Mathf.Clamp(value.x, min.x, max.x);
            value.y = Mathf.Clamp(value.y, min.y, max.y);
            value.z = Mathf.Clamp(value.z, min.z, max.z);
        }

        public static Vector3 TryUpdateAndClamp(this Vector3 current, Vector3 delta, Vector3 max)
        {
            current.TryUpdateAndClampThis(delta, max);
            return current;
        }

        public static void TryUpdateAndClampThis(this ref Vector3 current, Vector3 delta, Vector3 max)
        {
            current.x.TryUpdateAndClampThis(delta.x, max.x);
            current.y.TryUpdateAndClampThis(delta.y, max.y);
            current.z.TryUpdateAndClampThis(delta.z, max.z);
        }

        public static float TryUpdateAndClamp(this float current, float delta, float max)
        {
            current.TryUpdateAndClampThis(delta, max);
            return current;
        }

        public static void TryUpdateAndClampThis(this ref float current, float delta, float max)
        {
            if (max == 0f) return;

            if (max > 0f) {
                if (max > current) {
                    current = Mathf.Min(current += delta, max);
                }

            } else {
                if (max < current) {
                    current = Mathf.Max(current += delta, max);
                }
            }
        }
    }
}