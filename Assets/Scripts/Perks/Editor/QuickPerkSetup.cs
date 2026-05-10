using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Entropy.Perks.Editor
{
    public class QuickPerkSetup : EditorWindow
    {
        [MenuItem("Entropy/Quick Setup Perk Manager")]
        static void Open() => GetWindow<QuickPerkSetup>("Quick Setup");

        void OnGUI()
        {
            EditorGUILayout.LabelField("Quick Perk Manager Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "This creates a PerksManager on the player and auto-populates all perks.\n" +
                "Select your PLAYER GameObject first, then click below.",
                MessageType.Info);

            EditorGUILayout.Space(8);

            var selected = Selection.activeGameObject;
            EditorGUILayout.LabelField("Selected:", selected != null ? selected.name : "Nothing selected");

            EditorGUILayout.Space(8);

            GUI.enabled = selected != null;
            if (GUILayout.Button("Setup PerksManager Here", GUILayout.Height(32)))
            {
                SetupOnObject(selected);
            }
            GUI.enabled = true;

            EditorGUILayout.Space(16);

            if (GUILayout.Button("Find Existing PerksManager", GUILayout.Height(24)))
            {
                var existing = Object.FindAnyObjectByType<PerksManager>();
                if (existing != null)
                {
                    Selection.activeObject = existing.gameObject;
                    EditorGUIUtility.PingObject(existing.gameObject);
                }
                else
                {
                    EditorUtility.DisplayDialog("Not Found", "No PerksManager in scene. Select your player and click Setup.", "OK");
                }
            }
        }

        private void SetupOnObject(GameObject target)
        {
            Undo.RegisterCompleteObjectUndo(target, "Setup PerksManager");

            var manager = target.GetComponent<PerksManager>();
            if (manager == null)
                manager = target.AddComponent<PerksManager>();

            if (manager.AvailablePerks == null)
                manager.AvailablePerks = new();
            else
                manager.AvailablePerks.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Perks" });
            int count = 0;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var perk = prefab.GetComponent<PerkBase>();
                if (perk == null) continue;

                if (path.Contains("SprintCoilsPerk") || path.Contains("QuickFeedPerk") || path.Contains("MoonLegsPerk"))
                    continue;

                manager.AvailablePerks.Add(perk);
                count++;
            }

            EditorUtility.SetDirty(manager);
            EditorUtility.DisplayDialog("Done", $"PerksManager added to {target.name} with {count} perks.", "OK");
            Selection.activeObject = manager;
        }
    }
}
