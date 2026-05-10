using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entropy.Perks;
using Entropy.Perks.UI;
using UnityEngine;

namespace Entropy.Player
{
    public class PlayerRespawnManager : MonoBehaviour
    {
        public static PlayerRespawnManager Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float _respawnDelay = 0.3f;

        private Health _playerHealth;
        private bool _isRespawning;
        private DeathPerkSelectorUI _selectorUI;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _selectorUI = gameObject.GetComponent<DeathPerkSelectorUI>();
            if (_selectorUI == null)
                _selectorUI = gameObject.AddComponent<DeathPerkSelectorUI>();
        }

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerHealth = player.GetComponent<Health>();

            if (_playerHealth != null)
                _playerHealth.OnDeath.AddListener(OnPlayerDied);
        }

        void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath.RemoveListener(OnPlayerDied);
        }

        public void OnPlayerDied()
        {
            if (_isRespawning) return;
            _isRespawning = true;

            Time.timeScale = 0f;

            var candidates = GetAvailablePerks();
            Debug.Log($"[PlayerRespawnManager] {candidates.Count} perks available, {PerksManager.Instance?.ActivePerks.Count} active");

            var choices = PickRandom(candidates, Mathf.Min(3, candidates.Count));
            Debug.Log($"[PlayerRespawnManager] Showing {choices.Count} perk choices");

            if (_selectorUI != null)
                _selectorUI.Show(choices, OnPerkSelected, OnRefreshChoices);
        }

        private void OnPerkSelected(string perkID)
        {
            if (_selectorUI != null)
                _selectorUI.Hide();

            if (!string.IsNullOrEmpty(perkID) && PerksManager.Instance != null)
            {
                Debug.Log($"[PlayerRespawnManager] Granting perk: {perkID}");
                PerksManager.Instance.GrantPerk(perkID);
            }

            Time.timeScale = 1f;
            StartCoroutine(RespawnCoroutine());
        }

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSecondsRealtime(_respawnDelay);

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                _isRespawning = false;
                yield break;
            }

            var health = player.GetComponent<Health>();
            if (health != null)
                health.Revive();

            var spawn = PlayerSpawnPoint.Instance;
            if (spawn != null)
            {
                player.transform.position = spawn.SpawnPosition;
                player.transform.rotation = spawn.SpawnRotation;
            }

            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            _isRespawning = false;
        }

        private void OnRefreshChoices()
        {
            if (_selectorUI == null) return;

            _selectorUI.Hide();

            var candidates = GetAvailablePerks();
            var choices = PickRandom(candidates, Mathf.Min(3, candidates.Count));
            _selectorUI.Show(choices, OnPerkSelected, OnRefreshChoices);
        }

        private List<PerkBase> GetAvailablePerks()
        {
            if (PerksManager.Instance == null)
            {
                Debug.LogError("[PlayerRespawnManager] PerksManager.Instance is NULL!");
                return new List<PerkBase>();
            }

            var all = PerksManager.Instance.AvailablePerks;
            if (all == null || all.Count == 0)
            {
                Debug.LogError("[PlayerRespawnManager] PerksManager.AvailablePerks is empty! Assign perk prefabs in the Inspector.");
                return new List<PerkBase>();
            }

            var valid = all.Where(p => p != null).ToList();
            Debug.Log($"[PlayerRespawnManager] AvailablePerks pool: {valid.Count} total, {PerksManager.Instance.ActivePerks.Count} currently active");
            return valid;
        }

        private List<PerkBase> PickRandom(List<PerkBase> pool, int count)
        {
            var result = new List<PerkBase>();
            var copy = new List<PerkBase>(pool);

            while (copy.Count > 0 && result.Count < count)
            {
                int idx = Random.Range(0, copy.Count);
                result.Add(copy[idx]);
                copy.RemoveAt(idx);
            }

            return result;
        }
    }
}
