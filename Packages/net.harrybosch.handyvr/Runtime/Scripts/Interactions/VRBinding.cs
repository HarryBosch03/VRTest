using System;
using UnityEngine;

namespace HandyVR.Interactions
{
    /// <summary>
    /// Class used to track a binding between a <see cref="bindable">Bindable Object</see> and a Pose.
    /// </summary>
    public class VRBinding
    {
        public readonly VRBindable bindable;
        public bool active;

        private Vector3 lastPosition;
        private Vector3 lastValue;
        
        public readonly Func<Vector3> position;
        public readonly Func<Quaternion> rotation;
        public readonly Func<bool> flipped;
        
        /// <summary>
        /// Creates binding between a bindable object and a Pose Driver.
        /// [ IF YOU CAN, BINDINGS SHOULD BE CONSTRUCTED THROUGH <see cref="VRBindable.CreateBinding"/> ]
        /// </summary>
        /// <param name="bindable">Object that will be bound</param>
        /// <param name="position">Position the object will be bound to</param>
        /// <param name="rotation">Rotation the object will be bound to</param>
        /// <param name="flipped">Whether the object should be flipped.</param>
        public VRBinding(VRBindable bindable, Func<Vector3> position, Func<Quaternion> rotation, Func<bool> flipped)
        {
            if (bindable.ActiveBinding) bindable.ActiveBinding.Deactivate();

            this.position = position;
            this.rotation = rotation;
            this.flipped = flipped;
            
            this.bindable = bindable;
            active = true;

            bindable.OnBindingActivated(this);
        }

        /// <summary>
        /// Deactivates the binding, freeing the object bound.
        /// A binding cannot be reactivated, to rebind create another binding.
        /// </summary>
        public void Deactivate()
        {
            bindable.OnBindingDeactivated(this);
            
            if (!active) return;
            active = false;
        }

        /// <summary>
        /// Returns if the binding is active and valid.
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            if (!active) return false;
            if (!bindable) return false;

            return true;
        }

        /// <summary>
        /// Cast used to check if the binding is still doing stuff. Is null safe.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static implicit operator bool(VRBinding binding)
        {
            return binding != null && binding.Valid();
        }
    }
}