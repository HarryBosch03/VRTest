using UnityEngine;

namespace HandyVR.Utility
{
    public static class Scene
    {
        private static GameObject group;
        
        public static void BreakHierarchyAndGroup(UnityEngine.Transform transform)
        {
            if (!group)
            {
                group = new GameObject("HandyVR");
                group.hideFlags = HideFlags.DontSave;
            }
            
            transform.SetParent(group.transform);
        }

        public static void Group(UnityEngine.Transform transform)
        {
            var t = transform;
            while (t.parent)
            {
                t = t.parent;
            }
            BreakHierarchyAndGroup(t);
        }
    }
}