using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandyVR.Interactions
{
    /// <summary>
    /// Behaviour for sockets that <see cref="VRPickup">Pickups</see> can be put in.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("HandyVR/Socket", Reference.AddComponentMenuOrder.Components)]
    public sealed class VRSocket : MonoBehaviour
    {
        [Tooltip("Collision radius used to check for Pickups")]
        [SerializeField] private float searchRadius = 0.04f;
        [Tooltip("Whether the list of types acts as a blacklist or a whitelist.\n  Whitelist: Block Pickups without a type in the list\n  Blacklist: Block Pickups with a type in the list")]
        [SerializeField] private ListMode listMode = ListMode.Whitelist;
        [Tooltip("List of types to check against to allow binding.")]
        [SerializeField] private List<VRBindingType> list = new();
        [Tooltip("Behaviour if the object being bound has no type.")]
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
        }

        /// <summary>
        /// Searches scene through a sphere check for a Pickup within range.
        /// </summary>
        private void CheckForNewBinding()
        {
            var queryList = Physics.OverlapSphere(transform.position, searchRadius);
            foreach (var query in queryList)
            {
                var pickup = query.GetComponentInParent<VRPickup>();
                if (!pickup) continue;
                if (pickup.ActiveBinding) continue;
                if (!Filter(pickup)) continue;

                activeBinding = pickup.CreateBinding(() => transform.position, () => transform.rotation, () => false);
                
                break;
            }
        }

        /// <summary>
        /// Used to filter Pickups before binding.
        /// </summary>
        /// <param name="pickup"></param>
        /// <returns>Returns true if the element should be used</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
            Blacklist, // Only use if no Match.
            Whitelist, // Only Use if Match.
        }
    }
}
