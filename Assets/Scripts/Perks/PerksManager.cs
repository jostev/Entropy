using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entropy.Perks
{
    /// <summary>
    /// Central registry for the player's currently active perks.
    /// Survives scene transitions. Duplicates stack.
    /// </summary>
    public class PerksManager : MonoBehaviour
    {
        public static PerksManager Instance { get; private set; }

        [Header("Available Perk Prefabs")]
        public List<PerkBase> AvailablePerks;

        public List<IPerk> ActivePerks { get; private set; } = new();

        private IModdableStats _playerStats;

        /// <summary>
        /// Persists across scene transitions. Contains one entry per earned instance.
        /// Duplicates are represented by multiple identical IDs.
        /// </summary>
        private static List<string> _persistedPerkIDs = new();
        private bool _isRehydrating;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            _playerStats = GetComponentInParent<PlayerStats>();
            if (_playerStats == null)
            {
                Debug.LogError("PerksManager: No PlayerStats found on parent.");
                return;
            }

            // Rehydrate perks from previous scenes
            if (_persistedPerkIDs.Count > 0)
            {
                _isRehydrating = true;
                foreach (var id in _persistedPerkIDs.ToList())
                {
                    InstantiateAndEquip(id);
                }
                _isRehydrating = false;
            }
        }

        /// <summary>
        /// Grants a perk by ID. Creates a new instance every time — duplicates stack.
        /// Persists the ID so it survives level transitions.
        /// </summary>
        public void GrantPerk(string perkID)
        {
            InstantiateAndEquip(perkID);

            if (!_isRehydrating)
                _persistedPerkIDs.Add(perkID);
        }

        private void InstantiateAndEquip(string perkID)
        {
            var prefab = AvailablePerks.Find(p => p.ID == perkID);
            if (prefab == null)
            {
                Debug.LogWarning($"Perk '{perkID}' not found in AvailablePerks.");
                return;
            }

            var instance = Instantiate(prefab, transform);
            instance.OnEquip(_playerStats);
            ActivePerks.Add(instance);
            Debug.Log($"Granted perk: {instance.Title}");
        }

        /// <summary>
        /// Returns true if at least one instance of the perk ID is active.
        /// </summary>
        public bool HasPerk(string perkID)
        {
            return ActivePerks.Any(p => p.ID == perkID);
        }

        /// <summary>
        /// Removes the first matching instance from ActivePerks.
        /// Does NOT affect persistence — use only for temporary removals.
        /// </summary>
        public void RemovePerkInstance(IPerk perk)
        {
            perk.OnRemove(_playerStats);
            ActivePerks.Remove(perk);
            if (perk is MonoBehaviour mb)
                Destroy(mb.gameObject);
        }

        /// <summary>
        /// Clears all active perks AND wipes scene persistence.
        /// Call on player death or run reset.
        /// </summary>
        public void ClearAllPerks()
        {
            foreach (var perk in ActivePerks)
            {
                perk.OnRemove(_playerStats);
                if (perk is MonoBehaviour mb) Destroy(mb.gameObject);
            }
            ActivePerks.Clear();
            _persistedPerkIDs.Clear();
        }
    }
}
