using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandyVR.Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRBindingPoint : MonoBehaviour
    {
        [SerializeField] private float searchRadius = 0.04f;
        [SerializeField] private ListMode listMode = ListMode.Whitelist;
        [SerializeField] private List<VRBindingType> list = new();
        [SerializeField] private ListMode nullTypeBehaviour;

        private VRBinding activeBinding;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.1f);
            Gizmos.DrawSphere(transform.position, searchRadius);
        }

        private void FixedUpdate()
        {
            if (!activeBinding) CheckForNewBinding();
            else UpdateBinding();
        }

        private void UpdateBinding()
        {
            activeBinding.Position = transform.position;
            activeBinding.Rotation = transform.rotation;
        }

        private void CheckForNewBinding()
        {
            var queryList = Physics.OverlapSphere(transform.position, searchRadius);
            foreach (var query in queryList)
            {
                var pickup = query.GetComponentInParent<VRPickup>();
                if (!pickup) continue;
                if (pickup.ActiveBinding) continue;
                if (!Filter(pickup)) continue;

                activeBinding = pickup.CreateBinding(false);
                
                break;
            }
        }

        // Returns true if the element should be used.
        public bool Filter(VRPickup pickup)
        {
            if (!pickup.BindingType)
            {
                return nullTypeBehaviour switch
                {
                    // Nulls are whitelisted, therefor we return true to keep it.
                    ListMode.Whitelist => true,
                    // Nulls are blacklisted, therefor we return false to discard it.
                    ListMode.Blacklist => false,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return listMode switch
            {
                // List contains the type, therefor we return false to discard it (Blacklist).
                ListMode.Blacklist => !list.Contains(pickup.BindingType),
                // List contains the type, therefor we return true to keep it (Whitelist).
                ListMode.Whitelist => list.Contains(pickup.BindingType),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public enum ListMode
        {
            Blacklist,
            Whitelist,
        }
    }
}
