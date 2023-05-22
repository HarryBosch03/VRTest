using UnityEngine;

namespace HandyVR.Switches
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class FloatDriven : MonoBehaviour
    {
        [SerializeField] private FloatDriver driver;
        [SerializeField] private float defaultValue;
        [SerializeField] private bool clampValue;
        [SerializeField] private Vector2 valueRange;
        
        public float Value { get; private set; }

        protected virtual void Awake()
        {
            Value = defaultValue;
        }

        protected virtual void Update()
        {
            if (driver) Value = driver.Value;
            if (clampValue)
            {
                Value = Mathf.Clamp(Value, valueRange.x, valueRange.y);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!driver) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, driver.transform.position);
        }
    }
}
