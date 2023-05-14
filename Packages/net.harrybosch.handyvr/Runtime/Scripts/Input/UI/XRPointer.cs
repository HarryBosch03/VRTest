using System.Collections.Generic;
using HandyVR.Player;
using HandyVR.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;

namespace HandyVR.Input.UI
{
    [RequireComponent(typeof(PlayerHand))]
    public class XRPointer : MonoBehaviour
    {
        [SerializeField] private GameObject cursor;
        [SerializeField] private float scrollSpeed = 1000.0f;
        
        private ExtendedPointerEventData pointerData;
        private PlayerHand hand;

        public HandInput.InputWrapper TriggerAction => hand.Input.Trigger;
        public HandInput.InputWrapper GripAction => hand.Input.Grip;
        public HandInput.InputWrapper ThumbstickXAction => hand.Input.ThumbstickX;
        public HandInput.InputWrapper ThumbstickYAction => hand.Input.ThumbstickY;
        public Transform PointRef => hand.PointRef;
        public static List<XRPointer> All { get; } = new ();

        private void Awake()
        {
            pointerData = new ExtendedPointerEventData(EventSystem.current);
            hand = GetComponent<PlayerHand>();
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void LateUpdate()
        {
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            if (!cursor) return;
            var hit = pointerData.pointerCurrentRaycast;
            if (!hit.isValid)
            {
                cursor.SetActive(false);
                return;
            }
            cursor.SetActive(true);
            cursor.transform.position = hit.worldPosition;
            cursor.transform.rotation = Quaternion.identity;
        }

        public ExtendedPointerEventData GetData()
        {
            pointerData.trackedDevicePosition = PointRef.position;
            pointerData.trackedDeviceOrientation = PointRef.rotation;
            pointerData.pointerType = UIPointerType.Tracked;
            pointerData.button = PointerEventData.InputButton.Left;
            pointerData.scrollDelta = new Vector2(-ThumbstickXAction.Value, ThumbstickYAction.Value) * scrollSpeed * Time.deltaTime;
            
            return pointerData;
        }
    }
}