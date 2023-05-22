using UnityEngine;

namespace HandyVR.Utility
{
    public static class Physics
    {
        public static void IgnoreCollision(GameObject a, GameObject b, bool ignore)
        {
            var acl = a.GetComponentsInChildren<Collider>(true);
            var bcl = b.GetComponentsInChildren<Collider>(true);

            foreach (var ac in acl)
            foreach (var bc in bcl)
            {
                UnityEngine.Physics.IgnoreCollision(ac, bc, ignore);
            }
        }
    }
}