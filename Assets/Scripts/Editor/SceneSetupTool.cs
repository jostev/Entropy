using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Entropy.Perks;
using Entropy.Perks.UI;
using Entropy.Player;
using Entropy.Enemies;

namespace Entropy.Editor
{
    public class SceneSetupTool : EditorWindow
    {
        [MenuItem("Entropy/Setup Current Scene")]
        static void Open() => GetWindow<SceneSetupTool>("Scene Setup");

        private Vector2 _scroll;
        private bool _setupPerkMenu = true;
        private bool _setupPerksManager = true;
        private bool _setupPlayerRespawn = true;
        private bool _setupEnemyRespawn = true;
        private bool _setupPlayer = true;
        private bool _setupEnemies = true;
        private bool _setupSpawnPoint = true;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Entropy Scene Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "This tool automatically configures your scene with all required systems. " +
                "Check what you want to set up and click 'Configure Scene'.",
                MessageType.Info);

            EditorGUILayout.Space(8);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _setupPerkMenu = EditorGUILayout.ToggleLeft("Perk Menu (P key)", _setupPerkMenu);
            _setupPerksManager = EditorGUILayout.ToggleLeft("Perks Manager (all available perks)", _setupPerksManager);
            _setupPlayerRespawn = EditorGUILayout.ToggleLeft("Player Respawn + Death Perk Selection", _setupPlayerRespawn);
            _setupEnemyRespawn = EditorGUILayout.ToggleLeft("Enemy Respawn + Buffs", _setupEnemyRespawn);
            _setupPlayer = EditorGUILayout.ToggleLeft("Player Components (Health, Stats)", _setupPlayer);
            _setupEnemies = EditorGUILayout.ToggleLeft("Enemy Components (AI, Health, Spawn Data)", _setupEnemies);
            _setupSpawnPoint = EditorGUILayout.ToggleLeft("Player Spawn Point", _setupSpawnPoint);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(16);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button("CONFIGURE SCENE", GUILayout.Height(40)))
            {
                ConfigureScene();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Clear Stripped/Broken Objects"))
            {
                CleanStrippedObjects();
            }
        }

        private void ConfigureScene()
        {
            int changes = 0;

            if (_setupPerkMenu)
                changes += SetupPerkMenu();

            if (_setupPerksManager)
                changes += SetupPerksManager();

            if (_setupPlayerRespawn)
                changes += SetupPlayerRespawn();

            if (_setupEnemyRespawn)
                changes += SetupEnemyRespawn();

            if (_setupPlayer)
                changes += SetupPlayer();

            if (_setupEnemies)
                changes += SetupEnemies();

            if (_setupSpawnPoint)
                changes += SetupSpawnPoint();

            EditorUtility.SetDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log($"[SceneSetup] Applied {changes} configuration changes.");
            EditorUtility.DisplayDialog("Scene Setup", $"Applied {changes} changes. Save the scene (Ctrl+S) to persist.", "OK");
        }

        private int SetupPerkMenu()
        {
            int c = 0;

            var existing = Object.FindAnyObjectByType<PerkMenuManager>();
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject("PerkMenuManager");
                go.AddComponent<PerkMenuManager>();
                c++;
            }

            var canvas = go.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                c++;
            }

            if (go.GetComponent<CanvasScaler>() == null)
            {
                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                c++;
            }

            if (go.GetComponent<GraphicRaycaster>() == null)
            {
                go.AddComponent<GraphicRaycaster>();
                c++;
            }

            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = go.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
                cg.interactable = false;
                c++;
            }

            var menuUI = go.GetComponentInChildren<PerkMenuUI>(true);
            if (menuUI == null)
            {
                var uiGO = new GameObject("PerkMenuUI", typeof(RectTransform));
                uiGO.transform.SetParent(go.transform, false);
                menuUI = uiGO.AddComponent<PerkMenuUI>();
                c++;
            }

            var manager = go.GetComponent<PerkMenuManager>();
            var so = new SerializedObject(manager);
            so.FindProperty("_menuUI").objectReferenceValue = menuUI;
            so.ApplyModifiedProperties();

            return c;
        }

        private int SetupPerksManager()
        {
            int c = 0;
            var existing = Object.FindAnyObjectByType<PerksManager>();
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject("PerksManager");
                go.AddComponent<PerksManager>();
                c++;
            }

            var pm = go.GetComponent<PerksManager>();
            var so = new SerializedObject(pm);
            var availProp = so.FindProperty("AvailablePerks");

            if (availProp.arraySize == 0)
            {
                var perkPrefabs = FindAllPerkPrefabs();
                availProp.arraySize = perkPrefabs.Count;
                for (int i = 0; i < perkPrefabs.Count; i++)
                {
                    availProp.GetArrayElementAtIndex(i).objectReferenceValue = perkPrefabs[i];
                }
                so.ApplyModifiedProperties();
                c += perkPrefabs.Count;
                Debug.Log($"[SceneSetup] Added {perkPrefabs.Count} perk prefabs to PerksManager.");
            }

            return c;
        }

        private int SetupPlayerRespawn()
        {
            int c = 0;
            var existing = Object.FindAnyObjectByType<PlayerRespawnManager>();
            if (existing != null) return c;

            var go = new GameObject("PlayerRespawnManager");
            go.AddComponent<PlayerRespawnManager>();
            go.AddComponent<DeathPerkSelectorUI>();
            c += 2;

            return c;
        }

        private int SetupEnemyRespawn()
        {
            int c = 0;
            var existing = Object.FindAnyObjectByType<EnemyRespawnManager>();
            if (existing != null) return c;

            var go = new GameObject("EnemyRespawnManager");
            go.AddComponent<EnemyRespawnManager>();
            c++;

            return c;
        }

        private int SetupPlayer()
        {
            int c = 0;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[SceneSetup] No GameObject with 'Player' tag found.");
                return c;
            }

            if (player.GetComponent<Health>() == null)
            {
                player.AddComponent<Health>();
                c++;
            }

            if (player.GetComponent<PlayerStats>() == null)
            {
                player.AddComponent<PlayerStats>();
                c++;
            }

            var healthBar = player.GetComponentInChildren<PlayerHealthBar>(true);
            if (healthBar == null)
            {
                var canvasGO = new GameObject("PlayerHealthCanvas", typeof(Canvas));
                canvasGO.transform.SetParent(player.transform, false);
                var canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                var healthBarComp = canvasGO.AddComponent<PlayerHealthBar>();
                var hbSo = new SerializedObject(healthBarComp);
                hbSo.FindProperty("targetHealth").objectReferenceValue = player.GetComponent<Health>();
                hbSo.ApplyModifiedProperties();
                c++;
            }

            return c;
        }

        private int SetupEnemies()
        {
            int c = 0;
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                if (enemy.GetComponent<Health>() == null)
                {
                    enemy.AddComponent<Health>();
                    c++;
                }

                if (enemy.GetComponent<EnemyController>() == null)
                {
                    enemy.AddComponent<EnemyController>();
                    c++;
                }

                if (enemy.GetComponent<EnemySpawnData>() == null)
                {
                    enemy.AddComponent<EnemySpawnData>();
                    c++;
                }

                if (enemy.GetComponent<EnemyPerkBuff>() == null)
                {
                    enemy.AddComponent<EnemyPerkBuff>();
                    c++;
                }

                var ec = enemy.GetComponent<EnemyController>();
                if (ec != null)
                {
                    var ecSo = new SerializedObject(ec);
                    var playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        ecSo.FindProperty("_player").objectReferenceValue = playerObj.transform;
                        ecSo.ApplyModifiedProperties();
                    }
                }
            }

            if (enemies.Length > 0)
                Debug.Log($"[SceneSetup] Configured {enemies.Length} enemies.");

            return c;
        }

        private int SetupSpawnPoint()
        {
            int c = 0;
            var existing = Object.FindAnyObjectByType<PlayerSpawnPoint>();
            if (existing != null) return c;

            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 pos = player != null ? player.transform.position : Vector3.zero;

            var go = new GameObject("PlayerSpawnPoint");
            go.transform.position = pos;
            go.AddComponent<PlayerSpawnPoint>();
            c++;

            return c;
        }

        private List<PerkBase> FindAllPerkPrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Perks" });
            var result = new List<PerkBase>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<PerkBase>(path);
                if (prefab != null)
                    result.Add(prefab);
            }
            return result;
        }

        private void CleanStrippedObjects()
        {
            int removed = 0;
            var allObjects = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                .GetRootGameObjects();

            foreach (var root in allObjects)
            {
                removed += CleanRecursive(root.transform);
            }

            Debug.Log($"[SceneSetup] Removed {removed} stripped/broken objects.");
            EditorUtility.DisplayDialog("Cleanup", $"Removed {removed} broken objects.", "OK");
        }

        private int CleanRecursive(Transform t)
        {
            int removed = 0;
            var children = new List<Transform>();
            for (int i = 0; i < t.childCount; i++)
                children.Add(t.GetChild(i));

            foreach (var child in children)
            {
                bool isStripped = child.name.Contains("stripped") ||
                    (child.GetComponents<Component>().Length <= 1 && child.childCount == 0);

                if (isStripped)
                {
                    DestroyImmediate(child.gameObject);
                    removed++;
                }
                else
                {
                    removed += CleanRecursive(child);
                }
            }

            return removed;
        }
    }
}
