using UnityEngine;

namespace Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public abstract class FloatDriver : MonoBehaviour
    {
        public virtual float Value { get; protected set; }
    }
}
