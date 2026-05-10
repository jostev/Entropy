using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Editor
{
    public class SceneCleanupTool : EditorWindow
    {
        [MenuItem("Entropy/Cleanup Broken Scene Objects")]
        static void Open() => GetWindow<SceneCleanupTool>("Scene Cleanup");

        private int _brokenFound;

        void OnGUI()
        {
            EditorGUILayout.LabelField("Scene Cleanup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Find & Remove Broken Objects", GUILayout.Height(40)))
            {
                Cleanup();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Last run: {_brokenFound} objects removed", EditorStyles.miniLabel);
        }

        private void Cleanup()
        {
            _brokenFound = 0;
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (var go in allObjects)
            {
                if (go == null) continue;

                bool hasMissingScript = false;
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null)
                    {
                        hasMissingScript = true;
                        break;
                    }
                }

                bool isBrokenUI = false;
                if (!hasMissingScript)
                {
                    var canvas = go.GetComponent<Canvas>();
                    var scaler = go.GetComponent<CanvasScaler>();
                    var raycaster = go.GetComponent<GraphicRaycaster>();

                    if (canvas != null && scaler != null && raycaster != null)
                    {
                        if (go.transform.childCount == 0 && canvas.enabled == false)
                        {
                            isBrokenUI = true;
                        }
                    }
                }

                if (hasMissingScript || isBrokenUI)
                {
                    Debug.Log($"[SceneCleanup] Removing broken object: {go.name}", go);
                    Undo.DestroyObjectImmediate(go);
                    _brokenFound++;
                }
            }

            Debug.Log($"[SceneCleanup] Removed {_brokenFound} broken objects from scene.");
        }
    }
}
