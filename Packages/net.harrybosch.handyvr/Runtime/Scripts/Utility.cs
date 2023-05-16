using System;
using System.Collections.Generic;
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

        public static bool Best<T>(IEnumerable<T> list, out T best, Func<T, float> getScore, float startingScore = 0.0f)
        {
            best = default;
            var result = false;
            var bestScore = startingScore;
            foreach (var element in list)
            {
                var score = getScore(element);
                if (score < bestScore) continue;

                best = element;
                bestScore = score;
                result = true;
            }

            return result;
        }
        
        public static T Best<T>(IEnumerable<T> list, Func<T, float> getScore, float startingScore = 0.0f) where T : class
        {
            T best = null;
            var bestScore = startingScore;
            foreach (var element in list)
            {
                var score = getScore(element);
                if (score < bestScore) continue;

                best = element;
                bestScore = score;
            }

            return best;
        }
    }
}