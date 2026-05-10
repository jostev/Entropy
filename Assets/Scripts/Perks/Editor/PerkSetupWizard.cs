using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Entropy.Perks.Editor
{
    public class PerkSetupWizard : EditorWindow
    {
        [MenuItem("Entropy/Setup Perk Prefabs")]
        static void Open() => GetWindow<PerkSetupWizard>("Setup Perks");

        private Vector2 _scroll;
        private bool _includeOld = false;

        void OnGUI()
        {
            EditorGUILayout.LabelField("Perk Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            var manager = Object.FindAnyObjectByType<PerksManager>();
            if (manager == null)
            {
                EditorGUILayout.HelpBox("No PerksManager found in scene.\nAdd one to the player GameObject or a persistent systems object.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Manager: {manager.name}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Assigned: {manager.AvailablePerks?.Count ?? 0}", EditorStyles.miniLabel, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            _includeOld = EditorGUILayout.Toggle("Include old naming (SprintCoilsPerk)", _includeOld);

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Auto-Populate All Perks", GUILayout.Height(32)))
            {
                Undo.RecordObject(manager, "Auto-populate perks");
                AutoPopulate(manager);
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Clear All", GUILayout.Height(24)))
            {
                Undo.RecordObject(manager, "Clear perks");
                manager.AvailablePerks.Clear();
                EditorUtility.SetDirty(manager);
            }

            EditorGUILayout.Space(8);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
            if (manager.AvailablePerks != null)
            {
                foreach (var perk in manager.AvailablePerks)
                {
                    if (perk == null)
                    {
                        EditorGUILayout.LabelField("[Missing Reference]", EditorStyles.miniLabel);
                        continue;
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"[{perk.Rarity}] {perk.Title}", GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField(perk.ID, EditorStyles.miniLabel, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void AutoPopulate(PerksManager manager)
        {
            if (manager.AvailablePerks == null)
                manager.AvailablePerks = new();
            else
                manager.AvailablePerks.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Perks" });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var perk = prefab.GetComponent<PerkBase>();
                if (perk == null) continue;

                if (!_includeOld && (path.Contains("SprintCoilsPerk") || path.Contains("QuickFeedPerk") || path.Contains("MoonLegsPerk")))
                    continue;

                manager.AvailablePerks.Add(perk);
            }

            EditorUtility.DisplayDialog("Done", $"Assigned {manager.AvailablePerks.Count} perks.", "OK");
        }
    }
}
