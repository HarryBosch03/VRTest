using UnityEngine;

namespace Interactions
{
    public abstract class VRHandle : VRBindable
    {
        public sealed override bool CanCreateDetachedBinding => false;
        
        public abstract Vector3 HandPosition { get; }
        public abstract Quaternion HandRotation { get; }
    }
}