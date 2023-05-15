using UnityEngine;

namespace HandyVR
{
    public static class Utility
    {
        public static float Remap(float v, float iMin, float iMax, float oMin, float oMax)
        {
            return Mathf.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, v));
        }

        public static void IgnoreCollision(GameObject a, GameObject b, bool ignore)
        {
            var acl = a.GetComponentsInChildren<Collider>(true);
            var bcl = b.GetComponentsInChildren<Collider>(true);

            foreach (var ac in acl)
            foreach (var bc in bcl)
            {
                Physics.IgnoreCollision(ac, bc, ignore);
            }
        }

        public static class Quaternion
        {
            public static UnityEngine.Quaternion Difference(UnityEngine.Quaternion a, UnityEngine.Quaternion b)
            {
                var a2 = UnityEngine.Quaternion.identity * UnityEngine.Quaternion.Inverse(a);
                var b2 = UnityEngine.Quaternion.identity * UnityEngine.Quaternion.Inverse(b);

                return b2 * UnityEngine.Quaternion.Inverse(a2);
            }
        }
    }
}