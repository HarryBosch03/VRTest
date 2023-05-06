using UnityEngine;

namespace Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRPickup : VRBindable
    {
        [SerializeField] private VRBindingType bindingType;
        
        public override bool CanCreateDetachedBinding => true;
        public VRBindingType BindingType => bindingType;
    }
}