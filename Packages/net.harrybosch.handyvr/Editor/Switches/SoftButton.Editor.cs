using System;
using HandyVR.Switches;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HandyVREditor.Switches
{
    [InitializeOnLoad]
    public static class SoftButtonWarning
    {
        static SoftButtonWarning()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView obj)
        {
            Handles.BeginGUI();
            var buttons = Object.FindObjectsOfType<SoftButton>();
            foreach (var button in buttons)
            {
                OnSceneGUI((SoftButton)button);
            }

            Handles.EndGUI();
        }

        private static void OnSceneGUI(SoftButton softButton)
        {
            var transform = softButton.transform;

            var style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.fontSize = 32;
            Handles.Label(transform.position, "OBSOLETE - PLEASE REMOVE", style);
        }
    }
}