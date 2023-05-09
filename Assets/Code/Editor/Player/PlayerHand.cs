using UnityEditor;
using UnityEngine;
using UnityEngine.SpatialTracking;
using VRTest;
using VRTest.Player;
using UEditor = UnityEditor.Editor;

namespace Editor.Player
{
    [CustomEditor(typeof(PlayerHand))]
    public class PlayerHandEditor : UEditor
    {
        private PlayerHand hand;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying) return;
            
            hand = target as PlayerHand;
            if (!hand) return;

            var hasIssues = !IsChildOfTrackedPoseDriver();
            hasIssues = !HasPointReference() || hasIssues;
            hasIssues = !HasModel() || hasIssues;

            if (!hasIssues)
            {
                EditorGUILayout.HelpBox("No Problems here :D", MessageType.Info);
            }
        }

        private bool IsChildOfTrackedPoseDriver()
        {
            if (hand.transform.parent && hand.transform.parent.GetComponent<TrackedPoseDriver>()) return true;
            if (hand.transform.parent && hand.transform.parent.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>())
            {
                EditorGUILayout.HelpBox("PlayerHand does not support Tracked Pose Driver (Input System)", MessageType.Error);
                return true;
            }
            EditorGUILayout.HelpBox("PlayerHand must be a child of a Tracked Pose Driver", MessageType.Error);
            return false;
        }

        private bool HasPointReference()
        {
            if (hand.transform.DeepFind("Point Ref")) return true;
            EditorGUILayout.HelpBox("PlayerHand must have a child named \"Point Ref,\" as a reference as to where the index finger is pointing", MessageType.Error);
            return false;
        }

        private bool HasModel()
        {
            if (hand.transform.DeepFind("Model")) return true;
            EditorGUILayout.HelpBox("PlayerHand must have a child named \"Model,\" that contains the hand's renderers and colliders.", MessageType.Error);

            return false;
        }
    }
}