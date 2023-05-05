using UnityEngine;
using UnityEngine.InputSystem;

public static class Extensions
{
    public static class NameComparisons
    {
        public delegate bool Delegate(string a, string b);

        public static string Simplify(string s) => s.Trim().ToLower().Replace(" ", "");

        public static bool Hard(string a, string b) => a == b;
        public static bool Soft(string a, string b) => Simplify(a) == Simplify(b);
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.TryGetComponent(out T res) ? res : gameObject.AddComponent<T>();
    }
    
    public static Transform DeepFind(this Transform transform, string name, NameComparisons.Delegate areEqual = null)
    {
        areEqual ??= NameComparisons.Soft;

        if (areEqual(transform.name, name)) return transform;
        foreach (Transform child in transform)
        {
            var r = child.DeepFind(name);
            if (r) return r;
        }
            
        return null;
    }

    public static bool State(this InputAction action) => action.ReadValue<float>() > 0.5f;
}