namespace HandyVR.Utility
{
    public static class Transform
    {
        public static UnityEngine.Quaternion Difference(UnityEngine.Quaternion a, UnityEngine.Quaternion b)
        {
            var a2 = UnityEngine.Quaternion.identity * UnityEngine.Quaternion.Inverse(a);
            var b2 = UnityEngine.Quaternion.identity * UnityEngine.Quaternion.Inverse(b);

            return b2 * UnityEngine.Quaternion.Inverse(a2);
        }
    }
}